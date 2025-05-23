using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTO.Exam
{
    public class ExamPublishDto
    {
        [Required]
        public decimal Score { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }
        [Required]
        public TimeSpan Duration { get; set; }
        [Required]
        public bool showResult { get; set; }

    }
}
