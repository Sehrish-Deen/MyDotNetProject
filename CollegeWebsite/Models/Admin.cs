using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CollegeWebsite.Models;

public partial class Admin
{
    public int Id { get; set; }
    [Required]
    [StringLength(15, MinimumLength = 3, ErrorMessage = "Name must be within 3 to 15 characters")]
    [RegularExpression(@"^[A-Za-z]+( [A-Za-z]+)*$", ErrorMessage = "Name can contain only letters and single spaces between words.")]

    public string? Name { get; set; }
    [Required]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid Email Address")]
    public string? Email { get; set; }
    [Required]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
   ErrorMessage = "Password must be at least 8 characters long and include uppercase, lowercase, digit, and special character.")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }
}
