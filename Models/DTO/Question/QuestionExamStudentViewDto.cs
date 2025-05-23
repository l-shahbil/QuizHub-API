using QuizHub.Models.DTO.Answer;

namespace QuizHub.Models.DTO.Question
{
    public class QuestionExamStudentViewDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public List<AnswerExamStudentView> answers { get; set; }
    }
}
