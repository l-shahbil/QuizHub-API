namespace QuizHub.Models.DTO.Question
{
    public class QuestionViewDto
    {
        public string QuestionId { get; set; }
        public string QuestionText { get; set; }
        public decimal Difficulty { get; set; }
        public decimal Discrimination { get; set; }
        public List<Answer> Answers { get; set; }
    }
}
