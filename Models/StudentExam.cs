using System.ComponentModel.DataAnnotations.Schema;

namespace QuizHub.Models
{
    public class StudentExam
    {
        public decimal Score { get; set; }
        public DateTime AttemptStartTime { get; set; }
        public DateTime AttemptEndTime { get; set; }
        public TimeSpan TimeComplation { get; set; }

        //Foreign Key
        public string userId { get; set; }
        [ForeignKey("userId")]
        public AppUser User { get; set; }
        public string examId { get; set; }
        [ForeignKey("examId")]
        public Exam Exam { get; set; }
    }
}
