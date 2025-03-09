using QuizHub.Models.DTO.Department;
using QuizHub.Models.DTO.Subject;

namespace QuizHub.Services.Admin_Services.Interface
{
    public interface ISubjectService
    {
        Task<List<SubjectViewDto>> GetAllSubjectsAsync();
        Task<SubjectViewDto> GetSubjectByIdAsync(int id);
        Task<SubjectViewDto> AddSubjectAsync(SubjectCreateDto model);

        Task<SubjectViewDto> EditSubjectAsync(int id, SubjectUpdateDto model);
        Task<bool> DeleteSubjectAsync(int id);
    }
}
