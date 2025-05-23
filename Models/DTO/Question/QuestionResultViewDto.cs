namespace QuizHub.Models.DTO.Question
{
    public class QuestionResultViewDto
    {
        public string questionText { get; set; }

        
        public int correctAnswerId {  get; set; }
        public string correctAnswer {  get; set; }
        public int? selectAnswerId { get; set; }
        public string? selectAnswer { get; set; }
    }
}
