using System;
using System.Collections.Generic;

namespace SchoolDB.Models;

public partial class Course
{
    public int CourseId { get; set; }

    public int? FkempId { get; set; }

    public string CourseName { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual Employee? Fkemp { get; set; }
}
