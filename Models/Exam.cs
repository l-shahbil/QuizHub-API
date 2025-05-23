using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizHub.Models
{
    public class Exam
    {
        public string Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }
        [Required]
        public int NumberOfEasyQuestions { get; set; }
        [Required]
        public int NumberOfMediumLevelQuestions { get; set; }
        [Required]
        public int NumberOfDifficultQuestions { get; set; }
        [Required]
        public decimal ClarityRangeFrom { get; set; }
        [Required]
        public decimal ClarityRangeTo { get; set; }

        public int questionCount { get; set; }

        //Foreign key
        public int SubjectId { get; set; }
        public string? UserId { get; set; }
        //Relationships
        [ForeignKey("SubjectId")]
        public Subject Subject { get; set; }
        public ICollection<ExamQuestion> ExamQuestions { get; set; }
        [ForeignKey("UserId")]
        public AppUser AppUser { get; set; }
        public ICollection<ClassExam> classExams { get; set; }
        public ICollection<StudentAnswers> studentAnswers { get; set; }
    }
}
