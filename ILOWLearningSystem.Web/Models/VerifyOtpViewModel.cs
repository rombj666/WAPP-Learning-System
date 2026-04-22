using System.ComponentModel.DataAnnotations;

namespace ILOWLearningSystem.Web.Models;

public class VerifyOtpViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Display(Name = "OTP Code")]
    public string Otp { get; set; } = string.Empty;
}