using System;
using System.Collections.Generic;

namespace CollegeWebsite.Models;

public partial class Course
{
    public int CourseId { get; set; }

    public string CourseName { get; set; } = null!;

    public string? CourseDescription { get; set; }

    public int DepartmentId { get; set; }

    public string? ImagePath { get; set; }

    public virtual Department Department { get; set; } = null!;
}
