namespace QuizHub.Models.DTO.Exam
{
    public class ExamAvalibleDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string subjectName { get; set; }
        public string userName { get; set; }
        public decimal score { get; set; }
        public bool isShowResult { get; set; }
        public TimeSpan duration { get; set; }
        public DateTime startExame { get; set; }
        public DateTime endExame { get; set; }
    }
}
