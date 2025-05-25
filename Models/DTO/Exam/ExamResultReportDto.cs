namespace QuizHub.Models.DTO.Exam
{
    public class ExamResultReportDto
    {
        public decimal AverageScore { get; set; }
        public decimal PassRate { get; set; }
        public decimal TopScore { get; set; }
        public decimal LowestScore { get; set; }

        public int TotalStudentsInClass { get; set; }
        public int StudentsWhoAttended { get; set; }
        public int StudentsWhoDidNotAttend { get; set; }
    }

}
