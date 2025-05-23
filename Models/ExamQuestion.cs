using System.ComponentModel.DataAnnotations.Schema;

namespace QuizHub.Models
{
    public class ExamQuestion
    {
        public decimal Score { get; set; }
        public string ExamId { get; set; }
        [ForeignKey("ExamId")]
        public Exam Exam { get; set; }
        public int QuestionId { get; set; }
        [ForeignKey("QuestionId")]
        public Question Question { get; set; }
    }
}
