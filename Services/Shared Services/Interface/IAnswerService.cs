using QuizHub.Models;
using QuizHub.Models.DTO.Answer;
using QuizHub.Models.DTO.Notification;

namespace QuizHub.Services.Shared_Services.Interface
{
    public interface IAnswerService
    {
        Task<AnswerViewDto> addAnswerAsync(string userEmail, int questionId, AnswerCreateDto model);
        Task addAnswersAsync(Question question, AnswerCreateDto model);
        Task<AnswerViewDto> EditAnswerAsync(string userEmail, int questionId, int answerId, AnswerEditDto model);
        Task<bool> DeleteAnswerAsync(string userEmail, int questionId, int answerId);
        bool ValidateAnswers(List<AnswerCreateDto> Answers);
    }
}
