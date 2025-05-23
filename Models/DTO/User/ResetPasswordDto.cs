using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTO.User
{
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "Old password is required.")]
        public string LastPassword { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Password confirmation is required.")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
