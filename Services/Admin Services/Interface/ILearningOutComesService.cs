
using QuizHub.Models.DTO.LearingOutComes;
using QuizHub.Models.DTO.Subject;

namespace QuizHub.Services.Admin_Services.Interface
{
    public interface ILearningOutComesService
    {
        Task<List<LearningOutComesViewDto>> GetAllLearningOutComesAsync(int subjectId);
        Task<LearningOutComesViewDto> GetLearningOutComesByIdAsync(int id);
        Task<LearningOutComesViewDto> AddLearningOutComesAsync(int subjectId,LearningOutComesCreateDto model);

        Task<LearningOutComesViewDto> EditLearningOutComesAsync(int id, LearningOutComesUpdateDto model);
        Task<bool> DeleteLearningOutComesAsync( int id);
    }
}
