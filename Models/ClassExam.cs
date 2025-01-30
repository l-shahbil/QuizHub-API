using System.ComponentModel.DataAnnotations.Schema;

namespace QuizHub.Models
{
    public class ClassExam
    {
        public decimal Score { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Duration { get; set; }


        public int ClassId { get; set; }
        [ForeignKey("ClassId")]
        public Class Class { get; set; }
        public string ExamId { get; set; }
        [ForeignKey("ExamId")]
        public Exam Exam { get; set; }
    }
}
