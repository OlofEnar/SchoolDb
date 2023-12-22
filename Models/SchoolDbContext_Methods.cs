using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.VisualBasic;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolDB.Models
{
    public class SchoolDbContext_Methods
    {
        //Method for printing and getting selection choice from user
        private static string GetSelectedChoice(Dictionary<int, string> choices, string title)
        {
            return AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(title)
                    .PageSize(10)
                    .HighlightStyle(ConsoleIO.textColour)
                    .AddChoices(choices.Values)
            );
        }

        public void DisplayEmployees()
        {
            List<Employee> employees;

            using (SchoolDbContext dbContext = new())
            {
                var choice = GetSelectedChoice(new Dictionary<int, string> 
                {
                    { 1, "Hämta alla" },
                    { 2, "Välj avdelning" }
                }, "Vilken personal vill du hämta?");

                if (choice == "Hämta alla")
                {
                    employees = dbContext.Employees.ToList();
                    AnsiConsole.MarkupLine($"\nHämtar all [yellow]personal[/]");
                }
                else
                {
                    //Makes a dictionary that pairs DepartmentId with DepartmentName
                    Dictionary<int, string> departments = dbContext.Departments.ToDictionary(ed => ed.DepartmentId, ed => ed.DepartmentName);

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
                table.AddColumns("Förnamn", "Efternamn", "Titel", "Lön", "Anställningsdatum");

                foreach (var emp in employees)
                {
                    //Adds rows
                    table.AddRow($"{emp.FirstName}", $"{emp.LastName}", $"{emp.Title}", $"{emp.Salary}", $"{emp.EmploymentDate}");
                }
                // Renders table to console
                AnsiConsole.Write(table);
            }
            ConsoleIO.Io.BacktoMain();
        }

        public void AddEmployee()
        {
            string teacherField = "";

            //Fixa validering av inputs
            using (var dbContext = new SchoolDbContext())
            {
                var EmpFname = AnsiConsole.Ask<string>("Fyll i [yellow]förnamn[/]:");
                var EmpLname = AnsiConsole.Ask<string>("Fyll i [yellow]efternamn[/]:");
                var EmpSSN = AnsiConsole.Ask<string>("Fyll i [yellow]personnummer[/]:");
                var EmpTitle = AnsiConsole.Ask<string>("Fyll i [yellow]titel[/]:");
                var EmpDate = AnsiConsole.Ask<DateOnly>("Fyll i [yellow]anställningsdatum[/]:");
                var EmpSalary = AnsiConsole.Ask<int>("Fyll i [yellow]lön[/]:");

                //Makes a dictionary that pairs DepartmentId with DepartmentName
                Dictionary<int, string> departments = dbContext.Departments.ToDictionary(ed => ed.DepartmentId, ed => ed.DepartmentName);

                //Sends the dictionary to method for selecting and printing
                var selectedDept = GetSelectedChoice(departments, "Välj avdelning");

                //Store selected Id to variable
                var deptId = departments.FirstOrDefault(x => x.Value == selectedDept).Key;

               if (selectedDept == "Lärare")
                {
                    //Ask for Teacher field if it is a teacher that is added
                    teacherField = AnsiConsole.Ask<string>("Fyll i [yellow]ämne[/]:");
                }

                var newEmployee = new Employee
                {
                    FirstName = EmpFname,
                    LastName = EmpLname,
                    EmployeeSsn = EmpSSN,
                    Title = EmpTitle,
                    EmploymentDate = EmpDate,
                    Salary = EmpSalary,
                    FkdepartmentId = deptId
                };

                //Add employee and save changes
                dbContext.Employees.Add(newEmployee);
                dbContext.SaveChanges();

                //Update progress
                AnsiConsole.Status()
                .Start($"\n\nUppdaterar databasen...", ctx =>
                {
                    Thread.Sleep(2000);

                    AnsiConsole.MarkupLine($"Lägger till {EmpFname} {EmpLname}...");
                    Thread.Sleep(2000);

                    ctx.Status("Nästan klart...");
                    Thread.Sleep(2000);

                    ctx.Spinner(Spinner.Known.Star);
                    ctx.SpinnerStyle(Style.Parse("green"));
                });
                Console.WriteLine("Databasen uppdaterad.");


                ConsoleIO.Io.BacktoMain();
            }
        }

        public void AddStudent()
        {
            //Fixa validering av inputs
            using (var dbContext = new SchoolDbContext())
            {
                var StudFname = AnsiConsole.Ask<string>("Fyll i [yellow]förnamn[/]:");
                var StudLname = AnsiConsole.Ask<string>("Fyll i [yellow]efternamn[/]:");
                var StudSSN = AnsiConsole.Ask<string>("Fyll i [yellow]personnummer[/]:");

                //Makes a dictionary that pairs Class Id with Class name
                Dictionary<int, string> studClasses = dbContext.StudentClasses.ToDictionary(sc => sc.StudentClassId, sc => sc.StudentClassName);

                //Sends the dictionary to method for selecting and printing
                var selectedClass = GetSelectedChoice(studClasses, "Välj klass");

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

                //Update progress
                AnsiConsole.Status()
                .Start($"\n\nUppdaterar databasen...", ctx =>
                {
                    Thread.Sleep(2000);

                    AnsiConsole.MarkupLine($"Lägger till {StudFname} {StudLname}...");
                    Thread.Sleep(2000);

                    ctx.Status("Nästan klart...");
                    Thread.Sleep(2000);

                    ctx.Spinner(Spinner.Known.Star);
                    ctx.SpinnerStyle(Style.Parse("green"));
                });
                Console.WriteLine("Databasen uppdaterad.");


                ConsoleIO.Io.BacktoMain();
            }
        }

        public void DisplayRecentGrades()
        {
            using (var dbContext = new SchoolDbContext())
            {
                //Store the date of last month
                DateOnly lastMonth = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1));

                //Joins Student Id and Course Id
                var recentGrades = from enrollment in dbContext.Enrollments
                                   where enrollment.GradeDate >= lastMonth
                                   join student in dbContext.Students on enrollment.FkstudentId equals student.StudentId
                                   join course in dbContext.Courses on enrollment.FkcourseId equals course.CourseId
                                   //join employee in dbContext.Employees on course.FkempId equals employee.EmpId

                                   select new
                                   {
                                       FirstName = student.FirstName,
                                       LastName = student.LastName,
                                       Grade = enrollment.Grade,
                                       //TeacherName = $"{employee.FirstName} {employee.LastName}",
                                       CourseName = course.CourseName,
                                       CourseCode = course.CourseCode
                                   };
                // Creates the table
                var table = new Table();

                // Adds columns
                table.AddColumns("Förnamn", "Efternamn", "Betyg", "Kursnamn", "Kurskod");

                foreach (var grade in recentGrades)
                {
                    //Adds rows
                    table.AddRow($"{grade.FirstName}", $"{grade.LastName}", $"{grade.Grade}", $"{grade.CourseName}", $"{grade.CourseCode}");
                }
                // Renders table to console
                AnsiConsole.Write(table);
            }
            ConsoleIO.Io.BacktoMain();

        }

        public void DisplayClass()
        {
            using (var dbContext = new SchoolDbContext())
            {
                    //Makes a dictionary with ClassId and ClassName for the menu choice
                    Dictionary<int, string> studClasses = dbContext.StudentClasses.ToDictionary(sc => sc.StudentClassId, sc => sc.StudentClassName);
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
                                student.FirstName,
                                student.LastName,
                                studentClass.StudentClassName,
                                studentClass.StudentClassCode,
                            })
                            .ToList();
                
                // Creates the table
                var table = new Table();

                // Adds columns
                table.AddColumns("FörNamn", "Efternamn", "Klassnamn", "Klasskod");

                foreach (var student in students)
                {
                    //Adds rows
                    table.AddRow($"{student.FirstName}", $"{student.LastName}", $"{student.StudentClassName}", $"{student.StudentClassCode}");
                }
                // Renders table to console
                AnsiConsole.Write(table);
            }
            ConsoleIO.Io.BacktoMain();
        }

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
                                studentClass.StudentClassName,
                                studentClass.StudentClassCode,
                            })
                            .ToList();
        

                // Creates the table
                var table = new Table();

                // Adds columns
                table.AddColumns("FörNamn", "Efternamn", "Klassnamn", "Klasskod");

                foreach (var student in students)
                {
                    //Adds rows
                    table.AddRow($"{student.FirstName}", $"{student.LastName}", $"{student.StudentClassName}", $"{student.StudentClassCode}");
                }
                // Renders table to console
                AnsiConsole.Write(table);
            }
            ConsoleIO.Io.BacktoMain();
        }

        //public void DisplayStudentEnrollments()
        //{
            //var studentsInACourse = dbContex.Enrollments
            //    .Where(c => c.FkcourseId == 2)
            //    .Select(s => s.FkstudentId);

            ////Visa vilka kurser en student går
            //int oneStudent = 1;
            //var allCoursesForStudent = dbContex.Enrollments
            //    .Where(s => s.FkstudentId == oneStudent)
            //    .Select(c => c.FkcourseId);

            //foreach (var items in allCoursesForStudent)
            //{
            //    Console.WriteLine($"{items.FirstName} {items.LastName} {items.Title} {items.Salary} {items.EmploymentDate}");
            //}
            //Console.ReadLine();

            //var studentCount = dbcontex.Courses
            //    .GroupJoin(dbcontex.Enrollments)
            //    course => course.CourseID
            //    enrollment => enrollment.FKCourseID,
            //    (course, enrollment => new { course = course, enrollmentCount = enrollment.Count() });
        //}

        //public void GetUserInputString(string input)
        //{
        //    while (true)
        //    {
        //        if (input.Any(c => char.IsLetterOrDigit(c)))

        //            Console.WriteLine("Ogiltig inmatning");
        //    }
        //}

        //Method for handling user input errors
        //public string GetUserInput()
        //{
        //    while (true)
        //    {
        //        string userInput = Console.ReadLine();

        //        if (userInput.Any(c => char.IsLetterOrDigit(c)))
        //        {
        //            Console.WriteLine("Ogiltig inmatning");
        //        }

        //            break;
        //        }
        //    }
        //}

        public void DisplayGrades()
        {
            using (var dbContext = new SchoolDbContext())
            {
                //Joins Enrollments and Courses to get Course info
                var courses = dbContext.Courses
                    .Join(
                        dbContext.Enrollments,
                        enroll => enroll.CourseId,
                        course => course.FkcourseId,
                        (course, employee) => new
                        {
                            CourseName = course.CourseName,
                            CourseCode = course.CourseCode,
                            //TeacherFName = employee.FirstName,
                            //TeacherLName = employee.LastName,
                            Grades = dbContext.Enrollments
                                .Where(enrollment => enrollment.FkcourseId == course.CourseId)
                                .Select(enrollment => enrollment.Grade)
                                .ToList()
                        })
                    .ToList();
                
                // Creates the table
                var table = new Table();

                // Adds columns
                table.AddColumns("Kurskod","Kursnamn","Lägsta","Högsta");

                foreach (var course in courses)
                {
                    //Adds rows
                    table.AddRow($"{course.CourseCode}", $"{course.CourseName}", $"{course.Grades.Min()}", $"{course.Grades.Max()}");
                }
                // Renders table to console
                AnsiConsole.Write(table);
            }
            ConsoleIO.Io.BacktoMain();
        }

        //public void DisplayEnrollments()
        //{
        //    using (var dbContext = new SchoolDbContext())
        //    {
        //        var enrollments = dbContext.Enrollments.ToList();

        //        foreach( var e in enrollments)
        //        {
        //            Console.WriteLine($"{e.FkcourseId}   {e.Grade}");
        //        }

        //    }
        //}
    }
}
