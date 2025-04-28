using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTO.Answer
{
    public class AnswerCreateDto
    {
        [Required, StringLength(50)]
        public string AnswerText { get; set; }
        [Required]
        public bool IsCorrect { get; set; }
    }
}
