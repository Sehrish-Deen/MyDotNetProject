using System.ComponentModel.DataAnnotations;

namespace CollegeWebsite.Models
{
    public class AdmissionFormViewModel : IValidatableObject
    {
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Student name is required")]
        public string StudentName { get; set; } = null!;

        [Required(ErrorMessage = "Father's name is required")]
        public string FatherName { get; set; } = null!;

        [Required(ErrorMessage = "Date of birth is required")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; } = null!;

        [Required(ErrorMessage = "Residential address is required")]
        public string ResidentialAddress { get; set; } = null!;

        [Required(ErrorMessage = "Permanent address is required")]
        public string PermanentAddress { get; set; } = null!;

        [Required(ErrorMessage = "Please select a department")]
        public int? DepartmentId { get; set; }

        [Required(ErrorMessage = "Admission for field is required")]
        public string AdmissionFor { get; set; } = null!;

        [Required(ErrorMessage = "Board/University is required")]
        public string BoardOrUniversity { get; set; } = null!;

        [Required(ErrorMessage = "Enrollment number is required")]
        public string EnrollmentNumber { get; set; } = null!;

        [Required(ErrorMessage = "Exam center is required")]
        public string ExamCenter { get; set; } = null!;

        [Required(ErrorMessage = "Stream is required")]
        public string Stream { get; set; } = null!;

        [Required(ErrorMessage = "Marks secured is required")]
        [Range(0, 1000, ErrorMessage = "Marks secured must be between 0 and 1000")]
        public decimal MarksSecured { get; set; }

        [Required(ErrorMessage = "Total marks is required")]
        [Range(1, 1000, ErrorMessage = "Marks secured must be between 1 and 1000")]
        public decimal MarksOutOf { get; set; }

        [Required(ErrorMessage = "Class obtained is required")]
        public string ClassObtained { get; set; } = null!;

        public string? SportsDetails { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Date of Birth must be less than today
            if (DateOfBirth.Date >= DateTime.Today)
            {
                yield return new ValidationResult(
                    "Date of birth must be a past date.",
                    new[] { nameof(DateOfBirth) }
                );
            }

            // MarksSecured <= MarksOutOf
            if (MarksSecured > MarksOutOf)
            {
                yield return new ValidationResult(
                    "Marks secured cannot be greater than total marks.",
                    new[] { nameof(MarksSecured) }
                );
            }
        }
    }
}
