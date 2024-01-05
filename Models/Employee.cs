using System;
using System.Collections.Generic;

namespace SchoolDB.Models;

public partial class Employee
{
    public int EmpId { get; set; }

    public int FkdepartmentId { get; set; }

    public string? EmployeeSsn { get; set; }

    public string? Title { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public DateOnly EmploymentDate { get; set; }

    public int? Salary { get; set; }

    public string? TeachingField { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual Department Fkdepartment { get; set; } = null!;
}
