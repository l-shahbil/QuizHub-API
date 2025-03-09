using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizHub.Models
{
    public class Question
    {
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string QuestionText { get; set; }
        [Required,StringLength(10)]
        public decimal Difficulty { get; set; }
        [Required, StringLength(10)]

        public decimal Discrimination { get; set; }


        //Foreign key
        public string userId { get; set; }

        //RelationShips
        public ICollection<Answer> Answers { get; set; }
        public ICollection<StudentAnswer> StudentAnswers { get; set; }
        [ForeignKey("userId")]
        public AppUser User { get; set; }
        public ICollection<ExamQuestion> ExamQuestions { get; set; }

    }
}
