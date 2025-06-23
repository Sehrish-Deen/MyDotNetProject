using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace CollegeWebsite.Models
{
    public class FacultyViewModel
    {
        public int FacultyId { get; set; }

        [Required]
        [StringLength(15, MinimumLength = 3, ErrorMessage = "Name must be within 3 to 15 characters")]
        [RegularExpression(@"^[A-Za-z]+( [A-Za-z]+)*$", ErrorMessage = "Name can contain only letters and single spaces between words.")]

        public string FacultyName { get; set; } = null!;
        [Required]
        public string? Qualification { get; set; }

        [Required]
        public string? Designation { get; set; }
        [Required]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid Email Address")]
        public string? Email { get; set; }
        [Required]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Please select a department")]
        public int? DepartmentId { get; set; }

        // For new uploaded image
     
        public IFormFile? Photo { get; set; }

        // For showing old image while editing
        public string? ExistingPhoto { get; set; }

        // For drop-down list
        public List<Department>? Departments { get; set; }
    }
}
