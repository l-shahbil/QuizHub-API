using QuizHub.Models.DTO.User.Student;
using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTO.Class
{
    public class ClassViewDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string SubAdminName { get; set; }
        public string SubjectName { get; set; }
        public List<StudentViewDto> Students { get; set; }
    }
}
