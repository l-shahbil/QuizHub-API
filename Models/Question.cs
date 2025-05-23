using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizHub.Models
{
    public class Question
    {
        public int Id { get; set; }
        [Required]
        public string QuestionText { get; set; }
        [Required]
        public decimal Difficulty { get; set; }
        [Required]

        public decimal Discrimination { get; set; }
        public DateTime CreatedAt { get; set; }


        //Foreign key
        public string userId { get; set; }
        public int learningOutComesId { get; set; }

        //RelationShips
        public ICollection<Answer> Answers { get; set; }
        public ICollection<StudentAnswers> StudentAnswers { get; set; }
        [ForeignKey("userId")]
        public AppUser User { get; set; }
        public ICollection<ExamQuestion> ExamQuestions { get; set; }
        [ForeignKey("learningOutComesId")]
        public LearningOutcomes leraningOutComes { get; set; }

    }
}
