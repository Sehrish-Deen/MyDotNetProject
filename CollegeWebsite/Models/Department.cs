using System;
using System.Collections.Generic;

namespace CollegeWebsite.Models;

public partial class Department
{
    public int DepartmentId { get; set; }

    public string DepartmentName { get; set; } = null!;

    public string? Description { get; set; }

    public string? ImagePath { get; set; }

    public virtual ICollection<AdmissionApplication> AdmissionApplications { get; set; } = new List<AdmissionApplication>();

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<Faculty> Faculties { get; set; } = new List<Faculty>();
}
