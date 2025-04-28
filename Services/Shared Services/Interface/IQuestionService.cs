using QuizHub.Models.DTO.Answer;
using QuizHub.Models.DTO.Question;

namespace QuizHub.Services.Shared_Services.Interface
{
    public interface IQuestionService
    {
        Task<QuestionViewDto> AddQuestionAsync(string userEmail, int departmentId, int learningOutComesId, QuestionCreateDto model, int? classId);        
        Task<QuestionViewDto> EditQuestionAsync(string userEmail, int questionId, QuestionUpdateDto model);
        Task<List<QuestionViewDto>> GetAllQuestion(string userEmail, int subjectId);
        Task<bool> DeleteQuestionAsync(string userEmail, int questionId);
        Task<QuestionViewDto> GetQuestionById( int questionId);
    }
}
