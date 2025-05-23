using QuizHub.Models.DTO.Question;

namespace QuizHub.Models.DTO.Exam
{
    public class ExamStudentSubmitDto
    {
        public string stdExamId {  get; set; }
        public List<QuestionStudentSubmitDto> questions { get; set; }
    }
}
