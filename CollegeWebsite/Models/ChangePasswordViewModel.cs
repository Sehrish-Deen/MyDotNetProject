using System.ComponentModel.DataAnnotations;

namespace CollegeWebsite.Models
{
    public class ChangePasswordViewModel
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
   ErrorMessage = "Password must be at least 8 characters long and include uppercase, lowercase, digit, and special character.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
   ErrorMessage = "Password must be at least 8 characters long and include uppercase, lowercase, digit, and special character.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
