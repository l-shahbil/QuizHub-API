using QuizHub.Models.DTO.Question;

namespace QuizHub.Models.DTO.Exam
{
    public class ExamResultViewDto
    {
        public string stdExamId { get; set; }
        public string examId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string SubjectName { get; set; }
        public decimal Score { get; set; }
        public TimeSpan duration { get; set; }
        public decimal StudentScore { get; set; }
        public DateTime AttemptStartTime { get; set; }
        public DateTime AttemptEndTime { get; set; }
        public TimeSpan TimeComplation { get; set; }
       public  List<QuestionResultViewDto> questionResultViewDtos { get; set; }
    }
}
