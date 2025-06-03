using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTO.Answer
{
    public class AnswerCreateDto
    {
        [Required]
        public string AnswerText { get; set; }
        [Required]
        public bool IsCorrect { get; set; }
    }
}
