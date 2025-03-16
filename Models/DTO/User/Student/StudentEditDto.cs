using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTO.User.Student
{
    public class StudentEditDto
    {
        public string? Email { get; set; }
        public string? PassWord { get; set; }
        [StringLength(50)]
        public string? FirstName { get; set; }
        [StringLength(100)]
        public string? LastName { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
    }
}
