using System.Text.RegularExpressions;
using SchoolDB.Models;
using Spectre.Console;

namespace SchoolDB
{
    // Class for handling most of the input and output to the console
    internal class ConsoleIO
    {
        public static ConsoleIO io;

        // Properties for holding the colour styles
        public static string textColour = "yellow";
        public static string primaryClr = "yellow";
        public static string secondaryClr = "yellow";
        // Dictionary Field for holding the Main menu
        public Dictionary<string, Action> MenuActions { get; private set; }
        private Panel header;

        // Singleton for easy access and init of Io
        public static ConsoleIO Io
        {
            get
            {
                if (io == null) { io = new ConsoleIO(); }
                return io;
            }
        }

        // Constructor for setting up main menu and header
        private ConsoleIO()
        {
            SetupMenu();
            SetupHeader();
        }

        // Populates the dictionary for the main menu
        public void SetupMenu()
        {
            SchoolDbContext_Methods dbcontextMethods = new();

            //Dictionary with menu choices and its methods
            MenuActions = new Dictionary<string, Action>
            {
                { "Hämta personal", () => dbcontextMethods.DisplayEmployees() },
                { "Hämta klasslista", () => dbcontextMethods.DisplayClass() },
                { "Hämta alla studenter", () => dbcontextMethods.DisplayAllStudents() },
                { "Sätt betyg", () => SetGradesMenu() },
                { "Hämta betygssatta kurser", () => dbcontextMethods.DisplayGrades() },
                { "Hämta senaste betyg", () => dbcontextMethods.DisplayRecentGrades() },
                { "Hämta aktiva kurser", () => dbcontextMethods.DisplayActiveCourses() },
                { "Lägg till student", () => dbcontextMethods.AddStudent() },
                { "Lägg till Personal", () => dbcontextMethods.AddEmployee() },
                { "(Avsluta)", () => Environment.Exit(0) }
            };
        }

        //Menu for Setting grades
        public void SetGradesMenu()
        {
            SchoolDbContext_Methods dbcontextMethods = new();

            // Creates a dictionary for holding the submenu
            var submenuActions = new Dictionary<string, Action>
        {
            { "Välj student från lista", () => dbcontextMethods.SelectStudentToGrade() },
            //{ "Sök student", () => dbcontextMethods.SearchStudentToGrade() }, <-- Not implemented
            { "(Gå tillbaka)", () => BacktoMainWithPrompt(false) }
        };

            var choice = SchoolDbContext_Methods.GetSelectedChoice(submenuActions, "Hur vill du sätta betyg?");

            // Calls the chosen method
            submenuActions[choice]();
        }

        // Init the header
        public void SetupHeader()
        {
            // Create a panel for the logo and user information
            header = new Panel(
                Align.Center(
                    Logo()
                ));
            header.Header($"[[  Inloggad som: [{primaryClr}]Admin[/]  ]]", Justify.Center);
        }

        public void DisplayHeader()
        {
            AnsiConsole.Write(header.Expand());
        }

        // Displays Main menu and collects selection from user
        public void DisplayMenu()
        {
            //Prints the Menu
            var userChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Huvudmeny")
                    .HighlightStyle(textColour)
                    .PageSize(10)
                    .AddChoices(MenuActions.Keys));

            MenuActions[userChoice]();
        }

        // Displaying Header and Main Menu in one method
        public void MainScreen()
        {
            DisplayHeader();
            DisplayMenu();
        }

        // This is used for returning back to main menu, can be directly or with a prompt
        // depending on the bool argument
        public void BacktoMainWithPrompt(bool withPrompt)
        {
            if (withPrompt)
            {
                Console.Write("\nTryck för att gå tillbaka");
                Console.ReadLine();
                Console.Clear();
            }
            else
                Console.Clear();
            MainScreen();
        }

        // Gets a Yes or No from the user. Used in several methods when needed
        public bool AskYesNoQuestion(string question)
        {
            var answer = AnsiConsole.Prompt(
                new ConfirmationPrompt(question)
                    .Yes('j') // Sets the character that represents "yes"
                    .ChoicesStyle("grey42")
                    .HideDefaultValue()
                    .InvalidChoiceMessage("[red]Ogiltigt val, tryck 'j' för ja och 'n' för nej[/]")
                    );
            CleanUp();            
            return answer;
        }

        // Validates the names when adding employee/student details.
        // Checks so no wierd characters are included and also checks the length
        public string ValidateName(string question)
        {
            var result = AnsiConsole.Prompt(
                new TextPrompt<string>(question)
                    .ValidationErrorMessage("[red]Ogiltig input, försök igen[/]")
                    .Validate(name =>
                    {
                        return name switch
                        {
                            _ when !Regex.IsMatch(name, @"^[a-zåäöüA-ZÅÄÖÜ]+$") => ValidationResult.Error("[red]Ogiltigt tecken, försök igen[/]"),
                            _ when (name.Length < 2 ) => ValidationResult.Error("[red]Skriv ut hela namnet[/]"),
                            _ when (name.Length > 20) => ValidationResult.Error("[red]Oj det var långt! Har du möjligtvis ett kortare?[/]"),
                            _ => ValidationResult.Success(), 
                        };
                    }));
            if (result != null)
            {
                CleanUp();
            }

            return result;
        }

        // Validates the social security number when adding employee/student details.
        // Only accepts 12 digits.
        public string ValidateSSN(string question)
        {
            var result = AnsiConsole.Prompt(
                new TextPrompt<string>(question)
                    .ValidationErrorMessage("[red]Ogiltig input, försök igen[/]")
                    .Validate(ssn =>
                    {
                        return ssn switch
                        {
                            _ when ssn.Length != 12 || !ssn.All(char.IsDigit) => ValidationResult.Error("[red]Ogiltigt personnummer[/]"),
                            _ => ValidationResult.Success(),
                        };
                    }));
            if (result != null)
            {
                CleanUp();
            }

            return result;
        }

        // Validates the salary input when adding employee/student details.
        public int ValidateSalary(string question)
        {
            var result = AnsiConsole.Prompt(
                new TextPrompt<int>(question)
                    .ValidationErrorMessage("[red]Ogiltig input, försök igen[/]")
                    .Validate(numbers =>
                    {
                        return numbers switch
                        {
                            < 20000 => ValidationResult.Error("[red]Nu var ni snåla. Lite högre va?[/]"),
                            > 100000 => ValidationResult.Error("[red]Jisses! Nu går ni i konken. Sänk lönen[/]"),
                            _ => ValidationResult.Success(),
                        };
                    }));
            if (result != null)
            {
                CleanUp();
            }

            return result;
        }

        // Validates the grade from SelectStudentToGrade()
        public int ValidateGrade(string question)
        {
            var result = AnsiConsole.Prompt(
                new TextPrompt<int>(question)
                    .ValidationErrorMessage("[red]Ogiltig input, försök igen[/]")
                    .Validate(grade =>
                    {
                        return grade switch
                        {
                            > 5 => ValidationResult.Error("[red]Skriv in ett giltigt betyg (1-5)[/]"),
                            < 1 => ValidationResult.Error("[red]Skriv in ett giltigt betyg (1-5)[/]"),
                            _ => ValidationResult.Success(),
                        };
                    }));

            CleanUp();
            return result;
        }

        // Simulates an update progress. Just for fun.
        public void UpdateDatabaseProgress(string firstName, string lastName)
        {
            AnsiConsole.Status()
                .Start($"\n\nUppdaterar databasen...", ctx =>
                {
                    Thread.Sleep(2000);

                    AnsiConsole.MarkupLine($"Lägger till {firstName} {lastName}...");
                    Thread.Sleep(2000);

                    ctx.Status("Nästan klart...");
                    Thread.Sleep(2000);

                    ctx.Spinner(Spinner.Known.Star);
                    ctx.SpinnerStyle(Style.Parse("green"));
                });
            ConsoleIO.Io.CleanUp();
            Console.WriteLine("Databasen uppdaterad.");
        }

        public void CleanUp()
        {
            Console.Clear();
            DisplayHeader();
        }

        // Returns the logo, for the header.
        public Markup Logo()
        {
            return new Markup("\n########     ###    ########   #######  ##        #######   ######  ##    ##  #######  ##          ###    ##    ## \r\n" +
                            "##     ##   ## ##   ##     ## ##     ## ##       ##     ## ##    ## ##   ##  ##     ## ##         ## ##   ###   ## \r\n" +
                            "##     ##  ##   ##  ##     ## ##     ## ##       ##     ## ##       ##  ##   ##     ## ##        ##   ##  ####  ## \r\n" +
                            "########  ##     ## ########  ##     ## ##       ##     ##  ######  #####    ##     ## ##       ##     ## ## ## ## \r\n" +
                            "##        ######### ##   ##   ##     ## ##       ##     ##       ## ##  ##   ##     ## ##       ######### ##  #### \r\n" +
                            "##        ##     ## ##    ##  ##     ## ##       ##     ## ##    ## ##   ##  ##     ## ##       ##     ## ##   ### \r\n" +
                            "##        ##     ## ##     ##  #######  ########  #######   ######  ##    ##  #######  ######## ##     ## ##    ## \r\n");
        }
    }
}
