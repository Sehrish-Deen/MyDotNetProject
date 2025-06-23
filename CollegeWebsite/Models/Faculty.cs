using System;
using System.Collections.Generic;

namespace CollegeWebsite.Models;

public partial class Faculty
{
    public int FacultyId { get; set; }

    public string FacultyName { get; set; } = null!;

    public string? Qualification { get; set; }

    public string? Designation { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public int? DepartmentId { get; set; }

    public string? Photo { get; set; }

    public virtual Department? Department { get; set; }
}
