using System;
using System.Collections.Generic;

namespace SchoolDB.Models;

public partial class Student
{
    public int StudentId { get; set; }

    public string? StudentSsn { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public int FkstudentClassId { get; set; }

    public int? FkstudentAddressId { get; set; }

    public int? FkstudContactId { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual StudContactDetail? FkstudContact { get; set; }

    public virtual StudentAddress? FkstudentAddress { get; set; }

    public virtual StudentClass FkstudentClass { get; set; } = null!;
}
