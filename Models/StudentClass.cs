using System;
using System.Collections.Generic;

namespace SchoolDB.Models;

public partial class StudentClass
{
    public int StudentClassId { get; set; }

    public string StudentClassName { get; set; } = null!;

    public string? StudentClassSubName { get; set; }

    public int StudentClassCurrentYear { get; set; }

    public int StudentClassStartYear { get; set; }

    public int FkclassPrincipal { get; set; }

    public string StudentClassCode { get; set; } = null!;

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
