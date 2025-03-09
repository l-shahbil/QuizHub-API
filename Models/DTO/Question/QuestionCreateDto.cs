namespace QuizHub.Models.DTO.Question
{
    public class QuestionCreateDto
    {
        public string QuestionText { get; set; }
        public decimal Difficulty { get; set; }
        public decimal Discrimination { get; set; }
    }
}
