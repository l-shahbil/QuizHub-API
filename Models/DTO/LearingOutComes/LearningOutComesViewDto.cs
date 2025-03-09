using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuizHub.Models.DTO.Question;

namespace QuizHub.Models.DTO.LearingOutComes
{
    public class LearningOutComesViewDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public List<QuestionViewDto> questions { get; set; }
    }
}
