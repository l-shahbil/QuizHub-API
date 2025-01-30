using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTO.Authentication
{
    public class LoginDto
    {
        [Required]
        public string userName { get; set; }
        [Required]
        public string password { get; set; }
    }
}
