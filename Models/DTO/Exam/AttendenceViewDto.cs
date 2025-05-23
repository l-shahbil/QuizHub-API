namespace QuizHub.Models.DTO.Exam
{
    public class AttendenceViewDto
    {
        public string studentEamil {  get; set; }
        public string studentName { get; set; }
        public string examId { get; set; }
        public int classId { get; set; }
        public decimal score { get; set; }
    }
}
