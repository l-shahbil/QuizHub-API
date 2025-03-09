using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTO.Question
{
    public class QuestionUpdateDto
    {
        public string QuestionText { get; set; }
        [Required, StringLength(10)]
        public decimal Difficulty { get; set; }
        [Required, StringLength(10)]
        public decimal Discrimination { get; set; }

    }
}
