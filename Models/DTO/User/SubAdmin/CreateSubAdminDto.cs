using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTO.User.SubAdmin
{
    public class CreateSubAdminDto
    {
        [Required,EmailAddress]
        public string Email { get; set; }
        [Required,MinLength(8)]
        public string PassWord { get; set; }
        [Required, StringLength(50)]
        public string FirstName { get; set; }
        [Required, StringLength(100)]

        public string LastName { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

    }
}
