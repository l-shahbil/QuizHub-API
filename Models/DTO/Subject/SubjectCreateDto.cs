using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTO.Subject
{
    public class SubjectCreateDto
    {
        [Required, StringLength(50)]
        public string Name { get; set; }
        public string Description { get; set; }

    }
}
