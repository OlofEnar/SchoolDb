using System;
using System.Collections.Generic;

namespace SchoolDB.Models;

public partial class StudentAddress
{
    public int StudentAdressId { get; set; }

    public string? Street1 { get; set; }

    public string? Street2 { get; set; }

    public string? City { get; set; }

    public string? PostCode { get; set; }

    public int? FkstudentId { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
