using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTO.Subject
{
    public class SubjectUpdateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

    }
}
