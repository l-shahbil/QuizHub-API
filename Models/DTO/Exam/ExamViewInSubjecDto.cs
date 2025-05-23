namespace QuizHub.Models.DTO.Exam
{
    public class ExamViewInSubjecDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public string userName { get; set; }
        public int NumberOfEasyQuestions { get; set; }
        public int NumberOfMediumLevelQuestions { get; set; }
        public int NumberOfDifficultQuestions { get; set; }

        public decimal ClarityRangeFrom { get; set; }
        public decimal ClarityRangeTo { get; set; }


    }
}
