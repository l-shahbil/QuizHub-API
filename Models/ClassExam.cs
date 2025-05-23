using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizHub.Models
{
    public class ClassExam
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
        public bool showResult {  get; set; } 

        public int ClassId { get; set; }
        [ForeignKey("ClassId")]
        public Class Class { get; set; }
        public string ExamId { get; set; }
        [ForeignKey("ExamId")]
        public Exam Exam { get; set; }
        public List<StudentExam> StudentExam { get; set; }
    }
}
