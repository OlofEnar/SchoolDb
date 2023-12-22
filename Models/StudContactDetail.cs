using System;
using System.Collections.Generic;

namespace SchoolDB.Models;

public partial class StudContactDetail
{
    public int StudContactId { get; set; }

    public string? StudContactFirstName { get; set; }

    public string? StudContactLastName { get; set; }

    public string? StudContactNumber { get; set; }

    public string? StudContactRole { get; set; }

    public int? FkstudentId { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
