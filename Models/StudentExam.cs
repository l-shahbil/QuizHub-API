using System.ComponentModel.DataAnnotations.Schema;

namespace QuizHub.Models
{
    public class StudentExam
    {
        
            public string id { get; set; }
            public decimal Score { get; set; }
            public DateTime AttemptStartTime { get; set; }
            public DateTime AttemptEndTime { get; set; }
            public TimeSpan TimeComplation { get; set; }
            public string ExamType { get; set; }
            public bool isSubmit {  get; set; }

            //Foreign Key
            public string userId { get; set; }
            [ForeignKey("userId")]
            public AppUser User { get; set; }
            public int clsExamClassId { get; set; }
            public string clsExamExamId { get; set; }
            public ClassExam clsExam { get; set; }
        public List<StudentAnswers> studentAnswers { get; set; }

    }
}
