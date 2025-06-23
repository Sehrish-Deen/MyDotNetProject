using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CollegeWebsite.Models;

public partial class Feedback
{
    public int FeedbackId { get; set; }

    [Required]
    [RegularExpression(@"^[A-Za-z]+( [A-Za-z]+)*$", ErrorMessage = "Name can contain only letters and single spaces between words.")]

    public string Name { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string Subject { get; set; } = null!;

    [Required]
    public string Message { get; set; } = null!;

    public DateTime? SubmittedAt { get; set; }
}
