using System;
using System.Collections.Generic;

namespace CollegeWebsite.Models;

public partial class AdmissionApplication
{
    public int AdmissionId { get; set; }

    public int StudentId { get; set; }

    public string? StudentName { get; set; }

    public string FatherName { get; set; } = null!;

    public DateOnly DateOfBirth { get; set; }

    public string Gender { get; set; } = null!;

    public string ResidentialAddress { get; set; } = null!;

    public string PermanentAddress { get; set; } = null!;

    public int DepartmentId { get; set; }

    public string AdmissionFor { get; set; } = null!;

    public string BoardOrUniversity { get; set; } = null!;

    public string EnrollmentNumber { get; set; } = null!;

    public string ExamCenter { get; set; } = null!;

    public string Stream { get; set; } = null!;

    public decimal MarksSecured { get; set; }

    public decimal MarksOutOf { get; set; }

    public string ClassObtained { get; set; } = null!;

    public string? SportsDetails { get; set; }

    public DateTime? ApplicationDate { get; set; }

    public string? AdmissionStatus { get; set; }

    public string UniqueAdmissionNumber { get; set; } = null!;



    public virtual Department Department { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
