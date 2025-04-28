using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTO.Answer
{
    public class AnswerEditDto
    {
        public string AnswerText { get; set; }
        public bool IsCorrect { get; set; }
    }
}
