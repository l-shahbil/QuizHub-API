using QuizHub.Models;
using QuizHub.Models.DTO.Exam;
using QuizHub.Models.DTO.Question;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace QuizHub.Services.Shared_Services.Interface
{
    public interface IExamService
    {
        Task<ExamViewDto> CreateExamAsync (string userEmail, int departmentId,ExamCreateDto model);
        Task<List<ExamViewInSubjecDto>> GetAllExams(string userEmail, int subjectId, int departmentId);
        Task<bool> DeleteExamAsync(string userEmail, string examId, int departmentId);
        Task<ExamViewDto> UpdateExamAsynct(string userEmail, int departmentId, string examId, ExamUpdateDto model);
        Task<ExamViewDto> GetExamById(string userEmail, string examId, int departmentId);
        Task<bool> ExamPuplish(string userEmail, string examId, int classId,ExamPublishDto model);
        Task<bool> CancelExamPublication(string userEmail, int classId, string examId);
        Task<ExamStudentViewDto> ExamTake(string userEmail, int classId, string examId);
        Task<ExamResultViewDto> ExamSubmission(string userEmail, ExamStudentSubmitDto model);
        Task<bool> enableShowResult(string userEmail, int classId, string examId);
        Task<List<ExamPreviousViewDto>> GetExamPrevious(string studentEmail, int classId);
        Task<List<AttendenceViewDto>> DisplayExamAttendenceAndResults(string userEmail, int classId, string examId);

        //for student
        Task<List<ExamAvalibleDto>> GetExamsAvalibie(string userEmail, int classId);
        Task<List<ExamAvalibleDto>> getExamPublishedInClass(string userEmail, int classId);
        Task<ExamResultViewDto> ViewExamResult(string studentEmail, int classId, string examId);
        Task<ExamResultViewDto> ExamSubmissionPractices(string studentEmail, ExamStudentSubmitDto model);
        Task<ExamStudentViewDto> GetExamPractices(string studentEmail, int classId, List<int> learningOutcomeIds, int questionCount);
    }
}
