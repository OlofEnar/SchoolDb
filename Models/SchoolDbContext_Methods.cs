using Microsoft.IdentityModel.Tokens;
using Spectre.Console;


namespace SchoolDB.Models
{
    public class SchoolDbContext_Methods
    {
        // Method for printing and getting selection choice from user. It's used all over the
        // app for collecting choices from the user 
        public static string GetSelectedChoice(Dictionary<int, string> choices, string title)
        {
            return AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(title)
                    .PageSize(10)
                    .HighlightStyle(ConsoleIO.textColour)
                    .AddChoices(choices.Values)
            );
        }

        // Another version for the submenus, with methods as values
        public static string GetSelectedChoice(Dictionary<string, Action> choices, string title)
        {
            return AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(title)
                    .PageSize(10)
                    .HighlightStyle(ConsoleIO.textColour)
                    .AddChoices(choices.Keys)
            );
        }

        // Fetches either all employees or by department and prints them
        public void DisplayEmployees()
        {
            List<Employee> employees;

            using (SchoolDbContext dbContext = new())
            {
                // Makes a dictionary of the menu items and sends it to method for selecting
                var choice = GetSelectedChoice(new Dictionary<int, string> 
                {
                    { 1, "Hämta alla" },
                    { 2, "Välj avdelning" },
                    { 3, "(Gå tillbaka)" }
                }, "Vilken personal vill du hämta?");

                if (choice == "(Gå tillbaka)")
                {
                    ConsoleIO.Io.BacktoMainWithPrompt(false);
                }

                if (choice == "Hämta alla")
                {
                    //Adds all employees to a list
                    employees = dbContext.Employees.ToList();
                    AnsiConsole.MarkupLine($"\nHämtar all [yellow]personal[/]");
                }
                else
                {
                    //Makes a dictionary that pairs DepartmentId with DepartmentName
                    Dictionary<int, string> departments = dbContext.Departments.
                        ToDictionary(ed => ed.DepartmentId, ed => ed.DepartmentName);

                    //Sends the dictionary to method for selecting and printing
                    var selectedDept = GetSelectedChoice(departments, "Välj avdelning");
                    //Store selected Id to variable
                    var deptId = departments.FirstOrDefault(x => x.Value == selectedDept).Key;

                    //Filter and collect only the employees with the chosen department id
                    employees = dbContext.Employees.Where(e => e.FkdepartmentId == deptId).ToList();
                    AnsiConsole.MarkupLine($"\nHämtar all personal från [yellow]{selectedDept}[/]");
                }
                // Creates the table
                var table = new Table();

                // Adds columns
                table.AddColumns("Förnamn", "Efternamn", "Titel", "Lön", "År på skolan", "Anställningsdatum");

                foreach (var emp in employees)
                {
                    string titleCheck;

                    if (emp.FkdepartmentId == 2) 
                    {
                        titleCheck = $"{emp.Title} - {emp.TeachingField}";
                    } 
                    else { titleCheck = $"{emp.Title}"; }


                    //Adds rows
                    table.AddRow($"{emp.FirstName}", $"{emp.LastName}", $"{titleCheck}", $"{emp.Salary}", 
                        $"{Calc.CalcYears(emp.EmploymentDate, DateOnly.FromDateTime(DateTime.Now))}", $"{emp.EmploymentDate}");
                }
                // Renders table to console
                AnsiConsole.Write(table.Expand());
            }
            ConsoleIO.Io.BacktoMainWithPrompt(true);
        }

        // Method for adding a new employee
        public void AddEmployee()
        {
            string teacherField = "";
            string EmpTitle = "";

            // Gets all employee details from user through Validate methods
            using (var dbContext = new SchoolDbContext())
            {
                var EmpFname = ConsoleIO.Io.ValidateName("Fyll i [yellow]förnamn[/]"); 
                var EmpLname = ConsoleIO.Io.ValidateName("Fyll i [yellow]efternamn[/]:");
                var EmpSSN = ConsoleIO.Io.ValidateSSN("Fyll i [yellow]personnummer[/] [grey42](12 siffror utan bindestreck[/]):");
                var EmpDate = AnsiConsole.Ask<DateOnly>("Fyll i [yellow]anställningsdatum[/] [grey42](XXXX-XX-XX)[/]:");
                ConsoleIO.Io.CleanUp();
                var EmpSalary = ConsoleIO.Io.ValidateSalary("Fyll i [yellow]lön[/] [grey42](XXXXX)[/]:");
                bool isTeacher = ConsoleIO.Io.AskYesNoQuestion("Är personen lärare?");
             
                if (isTeacher)
                {
                    //Ask for Teacher field if it is a teacher that is added
                    teacherField = AnsiConsole.Ask<string>("Fyll i [yellow]ämne[/] [grey42](tex Kemi)[/]:");
                    EmpTitle = "Lärare";
                }
                if (!isTeacher)
                {
                    EmpTitle = AnsiConsole.Ask<string>("Fyll i [yellow]titel[/] [grey42](tex: Ekonomiansvarig[/]):");
                }
                //Makes a dictionary that pairs DepartmentId with DepartmentName
                Dictionary<int, string> departments = dbContext.Departments.ToDictionary(ed => ed.DepartmentId, ed => ed.DepartmentName);

                //Sends the dictionary to method for selecting and printing
                var selectedDept = GetSelectedChoice(departments, "Välj avdelning");
                ConsoleIO.Io.CleanUp();

                //Store selected Id to variable
                var deptId = departments.FirstOrDefault(x => x.Value == selectedDept).Key;

                var newEmployee = new Employee
                {
                    FirstName = EmpFname,
                    LastName = EmpLname,
                    EmployeeSsn = EmpSSN.ToString(),
                    Title = EmpTitle,
                    EmploymentDate = EmpDate,
                    Salary = EmpSalary,
                    FkdepartmentId = deptId,
                    TeachingField = teacherField                   
                };

                //Add employee to database and save changes
                dbContext.Employees.Add(newEmployee);
                dbContext.SaveChanges();
                ConsoleIO.Io.CleanUp();

                // Databas update progress
                ConsoleIO.Io.UpdateDatabaseProgress(EmpFname, EmpLname);
                ConsoleIO.Io.BacktoMainWithPrompt(true);
            }
        }

        public void AddStudent()
        {
            
            using (var dbContext = new SchoolDbContext())
            {
                // Collects Student details and validates input through Validators
                var StudFname = ConsoleIO.Io.ValidateName("Fyll i [yellow]förnamn[/]");
                var StudLname = ConsoleIO.Io.ValidateName("Fyll i [yellow]efternamn[/]");
                var StudSSN = ConsoleIO.Io.ValidateSSN("Fyll i [yellow]personnummer[/] [grey42](12 siffror utan bindestreck[/]):");

                //Makes a dictionary that pairs Class Id with Class name
                Dictionary<int, string> studClasses = dbContext.StudentClasses.ToDictionary(sc => sc.StudentClassId, sc => sc.StudentClassName);

                //Sends the dictionary to method for selecting and printing
                var selectedClass = GetSelectedChoice(studClasses, "Välj klass");
                ConsoleIO.Io.CleanUp();

                //Store selected Id to variable
                var studClassId = studClasses.FirstOrDefault(x => x.Value == selectedClass).Key;

                var newStudent = new Student
                {
                    FirstName = StudFname,
                    LastName = StudLname,
                    StudentSsn = StudSSN,
                    FkstudentClassId = studClassId
                };

                //Add student and save changes
                dbContext.Students.Add(newStudent);
                dbContext.SaveChanges();
                ConsoleIO.Io.UpdateDatabaseProgress(StudFname, StudLname);
                ConsoleIO.Io.BacktoMainWithPrompt(true);
            }
        }

        // Displays all grades that's been set the last month
        public void DisplayRecentGrades()
        {
            using (var dbContext = new SchoolDbContext())
            {
                //Store the date of last month
                DateOnly lastMonth = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1));

                //Joins Student Id & Course Id with enrollments
                var recentGrades = from enrollment in dbContext.Enrollments
                                   where enrollment.GradeDate >= lastMonth
                                   join student in dbContext.Students on enrollment.FkstudentId equals student.StudentId
                                   join course in dbContext.Courses on enrollment.FkcourseId equals course.CourseId
                                   join employee in dbContext.Employees on course.FkempId equals employee.EmpId

                                   select new
                                   {
                                       student.FirstName,
                                       student.LastName,
                                       enrollment.Grade,
                                       enrollment.GradeDate,
                                       TeacherName = $"{employee.FirstName} {employee.LastName}",
                                       course.CourseName,
                                       course.CourseCode
                                   };
                // Creates the table
                var table = new Table();

                // Adds columns
                table.AddColumns("Förnamn", "Efternamn", "Betyg", "Kursnamn", "Kurskod", "Lärare", "Datum");

                foreach (var grade in recentGrades)
                {
                    //Adds rows
                    table.AddRow($"{grade.FirstName}", $"{grade.LastName}", $"{grade.Grade}", 
                        $"{grade.CourseName}", $"{grade.CourseCode}", $"{grade.TeacherName}", $"{grade.GradeDate}");
                }
                // Renders table to console
                AnsiConsole.Write(table.Expand());
            }
            ConsoleIO.Io.BacktoMainWithPrompt(true);

        }

        // Displays selected student class
        public void DisplayClass()
        {
            using (var dbContext = new SchoolDbContext())
            {
                // Makes a dictionary with all available student classes 
                Dictionary<int, string> studClasses = dbContext.StudentClasses.ToDictionary(sc => sc.StudentClassId, sc => sc.StudentClassCode);
                // Sends the dictionary to method for selecting
                var selectedClass = GetSelectedChoice(studClasses, "Välj klass");
                // Assigns selected class to variable
                var studClassId = studClasses.FirstOrDefault(x => x.Value == selectedClass).Key;


                //Join StudentClass and Students to get the Class info.
                //Also only collect the class with mathing Class Id 
                var students = dbContext.Students
                        .Where(s => s.FkstudentClassId == dbContext.StudentClasses
                            .Where(sc => sc.StudentClassId == studClassId)
                            .Select(sc => sc.StudentClassId)
                            .FirstOrDefault())
                        .Join(
                            dbContext.StudentClasses,
                            student => student.FkstudentClassId,
                            studentClass => studentClass.StudentClassId,
                            (student, studentClass) => new
                            {
                                student.FirstName,
                                student.LastName,
                                ClassName = studentClass.StudentClassName,
                                ClassCode = studentClass.StudentClassCode,
                                ClassYear = studentClass.StudentClassCurrentYear,
                                ClassSubName = studentClass.StudentClassSubName,
                                studentClass.StudentClassStartYear,
                            })
                            .ToList();

                // Creates the table
                var table = new Table();
                var header = new Table();



                header.AddColumns($"Linje: {selectedClass}", $"Antal elever: { students.Count()}");

                // Adds columns
                table.AddColumns("Förnamn", "Efternamn", "Klassnamn", "Klass", "Klasskod", "År", "Start");

                foreach (var student in students)
                {
                    //Adds rows
                    table.AddRow($"{student.FirstName}", $"{student.LastName}", $"{student.ClassName}", $"{student.ClassSubName}",
                        $"{student.ClassCode}", $"{student.ClassYear}", $"{student.StudentClassStartYear}");
                }

                AnsiConsole.Write(header.Expand());
                AnsiConsole.Write(table.Expand());
            }
            ConsoleIO.Io.BacktoMainWithPrompt(true);
        }

        // Displays all students
        public void DisplayAllStudents()
        {
            using (var dbContext = new SchoolDbContext())
            {
                    AnsiConsole.MarkupLine($"\nHämtar alla [yellow]studenter[/]");
                    
                    //Join StudentClass and Students to get the Class info
                    var students = dbContext.Students
                        .Join(
                            dbContext.StudentClasses,
                            student => student.FkstudentClassId,
                            studentClass => studentClass.StudentClassId,
                            (student, studentClass) => new
                            {
                                student.FirstName,
                                student.LastName,
                                ClassName = studentClass.StudentClassName,
                                ClassCode = studentClass.StudentClassCode,
                                ClassYear = studentClass.StudentClassCurrentYear,
                                ClassSubName = studentClass.StudentClassSubName,
                                studentClass.StudentClassStartYear,
                            })
                            .ToList();
        

                // Creates the table
                var table = new Table();

                // Adds columns
                table.AddColumns("Förnamn", "Efternamn", "Klassnamn", "Klass", "Klasskod", "År", "Start");

                foreach (var student in students)
                {
                    //Adds rows
                    table.AddRow($"{student.FirstName}", $"{student.LastName}", $"{student.ClassName}", $"{student.ClassSubName}",
                        $"{student.ClassCode}", $"{student.ClassYear}", $"{student.StudentClassStartYear}");
                }
                // Renders table to console
                AnsiConsole.Write(table.Expand());
            }
            ConsoleIO.Io.BacktoMainWithPrompt(true);
        }

        // Displays all active courses. <- Needs fixing. Also adds courses where grade's been set.
        public void DisplayActiveCourses()
        {
            using (var dbContext = new SchoolDbContext())
            {
                var activeCourses = (from course in dbContext.Courses
                                     join employee in dbContext.Employees on course.FkempId equals employee.EmpId
                                     join enrollment in dbContext.Enrollments on course.CourseId equals enrollment.FkcourseId
                                     join student in dbContext.Students on enrollment.FkstudentId equals student.StudentId
                                     join studentClass in dbContext.StudentClasses on student.FkstudentClassId equals studentClass.StudentClassId
                                     select new
                                     {
                                         course.CourseId,
                                         course.CourseName,
                                         course.CourseCode,
                                         TeacherName = $"{employee.FirstName} {employee.LastName}",
                                         studentClass.StudentClassCode
                                     }).Distinct(); // Using distinct to fetch one student for each class code, only for displaying info

                // Creates the table
                var table = new Table();

                // Adds columns
                table.AddColumns("Kurskod", "Kursnamn", "Lärare", "Klasskod");

                foreach (var course in activeCourses)
                {
                    // Adds rows
                    table.AddRow($"{course.CourseCode}", $"{course.CourseName}", $"{course.TeacherName}", $"{course.StudentClassCode}");
                }

                // Renders table to console
                AnsiConsole.Write(table.Expand());
            }

            ConsoleIO.Io.BacktoMainWithPrompt(true);
        }

        // Methods for displaying courses that's been graded and also calculates Min, Median and Max grade.
        // It feels uncomplete, because it is enough that one enrollment has a value
        // to be included. Ideally I should have implemented this different and have som checks when
        // a whole class has done the course.
        public void DisplayGrades()
        {
            using (var dbContext = new SchoolDbContext())
            {
                // Enrollments gets joined with Courses to acess course details,
                // also excludes all enrollments that is null.
                var gradesByCourse = dbContext.Enrollments
                    .Where(e => e.Grade != null)
                    .Join(
                        dbContext.Courses,
                        enrollment => enrollment.FkcourseId,
                        course => course.CourseId,
                        (enrollment, course) => new
                        {
                            course.CourseCode,
                            course.CourseName,
                            enrollment.Grade,
                            TeacherName = dbContext.Employees
                                .Where(emp => emp.EmpId == course.FkempId)
                                .Select(emp => $"{emp.FirstName} {emp.LastName}")
                                .FirstOrDefault()
                        })
                    .GroupBy(e => new { e.CourseCode, e.CourseName })
                    .Select(group => new
                    {
                        group.Key.CourseCode,
                        group.Key.CourseName,
                        MinGrade = group.Min(e => e.Grade),
                        MaxGrade = group.Max(e => e.Grade),
                        MedianGrade = Calc.Median(group.Select(e => e.Grade)),
                        group.First().TeacherName
                    })
                    .ToList();

                // Creates the table
                var table = new Table();

                // Adds columns
                table.AddColumns("Kurskod", "Kursnamn", "Lägsta", "Högsta", "Medel", "Lärare");

                foreach (var course in gradesByCourse)
                {
                    // Adds rows
                    table.AddRow($"{course.CourseCode}", $"{course.CourseName}", $"{course.MinGrade}", $"{course.MedianGrade}",
                        $"{course.MaxGrade}", $"{course.TeacherName}");
                }

                // Renders table to console
                AnsiConsole.Write(table.Expand());
            }

            ConsoleIO.Io.BacktoMainWithPrompt(true);
        }



        // Grade an enrollment by searching for the student manually
        public void SearchStudentToGrade()
        {

        }

        // Setting a grade from selection through student class
        public void SelectStudentToGrade()
        {
            using (var dbContext = new SchoolDbContext())
            {
                // Makes a dictionary with ClassId and ClassName for the menu choice
                Dictionary<int, string> studClasses = dbContext.StudentClasses.ToDictionary(sc => sc.StudentClassId, sc => sc.StudentClassCode);
                var selectedClass = GetSelectedChoice(studClasses, "Välj klass");
                var studClassId = studClasses.FirstOrDefault(x => x.Value == selectedClass).Key;

                //Join StudentClass and Students to get the Class info.
                //Also only collect the class with mathing Class Id 
                var students = dbContext.Students
                        .Where(s => s.FkstudentClassId == dbContext.StudentClasses
                            .Where(sc => sc.StudentClassId == studClassId)
                            .Select(sc => sc.StudentClassId)
                            .FirstOrDefault())
                        .Join(
                            dbContext.StudentClasses,
                            student => student.FkstudentClassId,
                            studentClass => studentClass.StudentClassId,
                            (student, studentClass) => new
                            {
                                studentName = $"{student.FirstName} {student.LastName}",
                                student.StudentId,
                                studentClass.StudentClassId
                            })
                            .ToList();

                // Fetches all students from selected class and adds to a dictionary
                var studentChoice = students.ToDictionary(student => student.StudentId,student => student.studentName);
                
                // Sends dictionary to method for selecting
                var selectedStudent = GetSelectedChoice(studentChoice, "Välj student");
                
                //Store student Id to variable
                var studentId = studentChoice.FirstOrDefault(x => x.Value == selectedStudent).Key;

                // Fetch enrollments with no grade for the selected Student
                var enrollments = dbContext.Enrollments
                    .Where(e => e.FkstudentId == studentId && e.Grade == null)
                    .Join(
                        dbContext.Courses,
                        enrollment => enrollment.FkcourseId,
                        course => course.CourseId,
                        (enrollment, course) => new
                        {
                            enrollment.EnrollmentId,
                            course.CourseName
                        })
                    .ToList();

                // Checks if the students has no enrollments
                if (enrollments.IsNullOrEmpty())
                {
                    AnsiConsole.WriteLine("Inga aktiva kurser att betygsätta hittades");
                    if (ConsoleIO.Io.AskYesNoQuestion("\nVill du välja en annan student från samma klass?"))
                    {
                        Console.WriteLine("Detta är en premiumfunktion...\nDvs jag hann inte fixat detta :p");
                        ConsoleIO.Io.BacktoMainWithPrompt(true);

                    } else { ConsoleIO.Io.BacktoMainWithPrompt(false);}
                }

                // Adds all enrollments without grade to a dicionary
                var courseChoice = enrollments.ToDictionary(e => e.EnrollmentId, e => e.CourseName);
                var selectedCourse = GetSelectedChoice(courseChoice, "Välj kurs");

                // Store enrollmentId to variable
                var selectedEnrollment = courseChoice.FirstOrDefault(x => x.Value == selectedCourse).Key;

                // Get the grade from user through validation
                int grade = ConsoleIO.Io.ValidateGrade(
                    $"Ange betyg för [yellow]{selectedStudent}[/] på kursen [yellow]{selectedCourse}[/] ([grey42]1-5)[/]:");

                // Adds selected enrollment to variable
                var enrollmentToUpdate = dbContext.Enrollments
                    //.Include(e => e.Fkstudent)
                    .FirstOrDefault(e => e.FkstudentId == studentId && e.EnrollmentId == selectedEnrollment);

                // Null check in case the enrollment wasn't found
                if (enrollmentToUpdate != null)
                {
                    // Sets the chosen grade to Grade property 
                    enrollmentToUpdate.Grade = grade;

                    // Updates the database
                    dbContext.Update(enrollmentToUpdate);
                    dbContext.SaveChanges();

                    // Summary
                    AnsiConsole.WriteLine($"\nBetyg [yellow][{grade}][/] satt för " +
                        $"[yellow]{selectedStudent}[/] på kursen [yellow]{selectedCourse}[/].");
                    ConsoleIO.Io.BacktoMainWithPrompt(true);

                }
                else
                {
                    ConsoleIO.Io.BacktoMainWithPrompt(true);
                }



            }
        }

        public void EnrollClass()
        {

        }

        public void EnrollStudent()
        {

        }

        public void AddnewCourse()
        {

        }

    }
}
