using System;
using System.Collections.Generic;

namespace SchoolDB.Models;

public partial class Enrollment
{
    public int EnrollmentId { get; set; }

    public int FkstudentId { get; set; }

    public int FkcourseId { get; set; }

    public int? Grade { get; set; }

    public DateOnly? GradeDate { get; set; }

    public virtual Course Fkcourse { get; set; } = null!;

    public virtual Student Fkstudent { get; set; } = null!;
}
