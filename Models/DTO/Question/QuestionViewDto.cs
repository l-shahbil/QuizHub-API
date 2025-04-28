using QuizHub.Models.DTO.Answer;

namespace QuizHub.Models.DTO.Question
{
    public class QuestionViewDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public decimal Difficulty { get; set; }
        public decimal Discrimination { get; set; }
        public List<AnswerViewDto> Answers { get; set; }
    }
}
