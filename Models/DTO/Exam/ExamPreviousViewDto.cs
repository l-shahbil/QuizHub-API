namespace QuizHub.Models.DTO.Exam
{
    public class ExamPreviousViewDto
    {
        public string examId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string SubjectName { get; set; }
        public decimal scoreExam { get; set; }
        public decimal ScoreStudent { get; set; }
        public string ExamType { get; set; }
        public DateTime AttemptStartTime { get; set; }
        public DateTime AttemptEndTime { get; set; }
        public TimeSpan TimeComplation { get; set; }
    }
}
