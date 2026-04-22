using System.ComponentModel.DataAnnotations;

namespace ILOWLearningSystem.Web.Models
{
    public class EditProfileViewModel
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? ProfileThemeColor { get; set; }
    }
}