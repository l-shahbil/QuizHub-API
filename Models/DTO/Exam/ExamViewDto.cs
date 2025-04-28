using QuizHub.Models.DTO.Question;

namespace QuizHub.Models.DTO.Exam
{
    public class ExamViewDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Subject { get; set; }
        public DateTime CreatedDate { get; set; }
        public string userName { get; set; }
        public List<string> learningOutComes { get; set; }
        public List<QuestionViewDto> questions { get; set; }
    }
}
