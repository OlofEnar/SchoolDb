using SchoolDB.Models;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolDB
{
    internal class ConsoleIO
    {
        public static ConsoleIO io;
        public static string textColour = "yellow";

        public static ConsoleIO Io
        {
            get
            {
                if (io == null) { io = new ConsoleIO(); }
                return io;
            }
        }

        public void Menu()
        {
            SchoolDbContext_Methods dbcontextMethods = new();

            //Dictionary with menu choices and its method
            var menuActions = new Dictionary<string, Action>
            {
                { "Hämta personal", () => dbcontextMethods.DisplayEmployees() },
                { "Hämta klasslista", () => dbcontextMethods.DisplayClass() },
                { "Hämta alla studenter", () => dbcontextMethods.DisplayAllStudents() },
                { "Hämta alla betyg", () => dbcontextMethods.DisplayGrades() },
                { "Hämta senaste betyg", () => dbcontextMethods.DisplayRecentGrades() },
                { "Lägg till student", () => dbcontextMethods.AddStudent() },
                { "Lägg till Personal", () => dbcontextMethods.AddEmployee() },

            };

            //Prints the Menu
            var userChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Huvudmeny")
                    .HighlightStyle(textColour)
                    .PageSize(10)
                    .AddChoices(menuActions.Keys));
            menuActions[userChoice]();
        }

        public void BacktoMain()
        {
            Console.Write("\nTryck för att gå tillbaka");
            Console.ReadLine();
            Console.Clear();
            Menu();
        }

    }
}
