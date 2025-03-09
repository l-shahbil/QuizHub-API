using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTO.User.Teacher
{
    public class UpdateTeacherDto
    {

        public string? Email { get; set; }
        public string? PassWord { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}
