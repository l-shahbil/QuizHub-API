using QuizHub.Models;
using QuizHub.Models.DTO.Exam;
using QuizHub.Models.DTO.Question;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace QuizHub.Services.Shared_Services.Interface
{
    public interface IExamService
    {
        Task<ExamViewDto> CreateExamAsync (string userEmail, int departmentId,ExamCreateDto model);
        //Task<ExamViewDto> EditExamAsync(string userEmail, int questionId, QuestionUpdateDto model);
        Task<List<ExamViewDto>> GetAllExams(string userEmail, int subjectId);
        Task<bool> DeleteExamAsync(string userEmail, int questionId);
        Task<ExamViewDto> GetExamById(int questionId);
        Task GetExamPrevious();
        Task<bool> ExamPuplish(string userEmail, string examId, int classId,ExamPublishDto model);
        Task<bool> CancelExamPublication(string userEmail, int classExamId);
        Task<ExamViewDto> ExamTake();
        Task ExamSubmission();
        Task DisplayExamAttendenceAndResults();

        //for student
        Task GetExamsAvalibie();
        Task ViewExamResult();
    }
}
