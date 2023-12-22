using System;
using System.Collections.Generic;

namespace SchoolDB.Models;

public partial class Teacher
{
    public int TeacherId { get; set; }

    public int FkempId { get; set; }

    public string? TeacherField { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual Employee Fkemp { get; set; } = null!;

    public virtual ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();
}
