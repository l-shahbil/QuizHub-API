using QuizHub.Models.DTO.Question;

namespace QuizHub.Models.DTO.Exam
{
    public class ExamStudentViewDto
    {
        public string stdExamId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string SubjectName { get; set; }
        public decimal Score { get; set; }
        public TimeSpan Duration { get; set; }
        public List<QuestionExamStudentViewDto> questions { get; set; }

    }
}
