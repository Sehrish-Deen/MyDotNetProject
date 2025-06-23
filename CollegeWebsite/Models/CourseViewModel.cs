using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CollegeWebsite.Models
{
    public class CourseViewModel
    {
        // Primary key of the course (used for editing existing courses)
        public int CourseId { get; set; }

        // Course name is required, display error message if empty
        [Required(ErrorMessage = "Course Name is required")]
        public string CourseName { get; set; } = null!;

        // Course description is required, display error message if empty
        [Required(ErrorMessage = "Course Description is required")]
        public string CourseDescription { get; set; } = null!;

        // DepartmentId is required and must be selected from dropdown
        [Required(ErrorMessage = "Please select a department")]
        public int DepartmentId { get; set; }

        // Optional image file uploaded for the course (e.g., course thumbnail)
        public IFormFile? ImageFile { get; set; }

        // List of departments to populate dropdown list in the view
        // This is not posted back, only used to display options
        public List<SelectListItem>? Departments { get; set; }
    }
}
