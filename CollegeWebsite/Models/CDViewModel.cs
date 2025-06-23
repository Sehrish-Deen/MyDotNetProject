using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CollegeWebsite.Models
{
    public class CDViewModel
    {
        // Department info
        public string DepartmentName { get; set; } = null!;
        public string? DepartmentDescription { get; set; }
        public string? DepartmentImagePath { get; set; }

        // Course info
        public string CourseName { get; set; } = null!;
        public string? CourseDescription { get; set; }
        public string? CourseImagePath { get; set; }

        // Faculties
        public List<Faculty> Faculties { get; set; } = new List<Faculty>();

        // Students
        public List<Student> Students { get; set; } = new List<Student>();

        // Feedbacks List
        public List<Feedback> Feedbacks { get; set; } = new List<Feedback>();

        // Feedback Form Input Binding
        public Feedback FeedbackInput { get; set; } = new Feedback();

    }
}
