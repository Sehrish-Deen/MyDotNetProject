using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CollegeWebsite.Models;

public partial class Student
{
    public int StudentId { get; set; }
    [Required]
    [RegularExpression(@"^[a-zA-Z ]+$", ErrorMessage = "Name can contain only letters and spaces.")]
    [StringLength(15, MinimumLength = 3, ErrorMessage = "Name must be within 3 to 15 characters")]

    public string StudentName { get; set; } = null!;
    [Required]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid Email Address")]
    public string Email { get; set; } = null!;
    [Required]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
   ErrorMessage = "Password must be at least 8 characters long and include uppercase, lowercase, digit, and special character.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    public virtual ICollection<AdmissionApplication> AdmissionApplications { get; set; } = new List<AdmissionApplication>();
}
