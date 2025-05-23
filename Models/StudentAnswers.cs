using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizHub.Models
{
    public class StudentAnswers
    {
        [Key]
        public int ID { get; set; }
        public Answer SelectAnswer { get; set; }
        public decimal Score { get; set; }
        public bool IsCorrect { get; set; }

        //Realtionships
        public string? userId { get; set; }
        [ForeignKey("userId")]
        public AppUser? User { get; set; }

        [ForeignKey("studentExamId")]
        public string studentExamId { get; set; }
        public StudentExam StudentExam { get; set; }

    }
}
