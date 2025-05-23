using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTO.Question
{
    public class QuestionUpdateDto
    {
        public string QuestionText { get; set; }
        public decimal Discrimination { get; set; }

    }
}
