using System.ComponentModel.DataAnnotations.Schema;

namespace QuizHub.Models
{
    public class StudentAnswer
    {
        public Answer SelectAnswer { get; set; }
        public decimal Score { get; set; }
        public bool IsCorrect { get; set; }

        //Realtionships
        public string? userId { get; set; }
        [ForeignKey("userId")]
        public AppUser? User { get; set; }
        public int? QuestionId { get; set; }
        [ForeignKey("QuestionId")]
        public Question? Question { get; set; }
        public string? ExamId { get; set; }
        [ForeignKey("ExamId")]
        public Exam? Exam { get; set; }

    }
}
