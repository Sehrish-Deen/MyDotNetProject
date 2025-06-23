using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CollegeWebsite.Models
{
    public class DepartmentViewModel
    {
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Department Name is required")]
        public string DepartmentName { get; set; } = null!;

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = null!;

        // File is optional during edit, required only during create if you want
        public IFormFile? ImagePath { get; set; }

        // For showing existing image in Edit view
        public string? ExistingImage { get; set; }

        // Optional – only include if used in your views/forms
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
