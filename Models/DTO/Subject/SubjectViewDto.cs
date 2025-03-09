using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTO.Subject
{
    public class SubjectViewDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> LearingOutComes { get; set; }

    }
}
