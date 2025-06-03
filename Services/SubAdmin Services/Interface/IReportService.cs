using QuizHub.Models.DTO.Exam;

namespace QuizHub.Services.SubAdmin_Services.Interface
{
    public interface IReportService
    {
        Task<ExamResultReportDto> getReportExam(string userEmail, int classId, string examId);
    }
}
