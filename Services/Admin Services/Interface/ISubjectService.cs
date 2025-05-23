using QuizHub.Models.DTO.Department;
using QuizHub.Models.DTO.Subject;

namespace QuizHub.Services.Admin_Services.Interface
{
    public interface ISubjectService
    {
        Task<List<SubjectViewDto>> GetAllSubjectsAsync(string userEmail);
        Task<SubjectViewDetailsDto> GetSubjectByIdAsync(int id);
        Task<List<SubjectViewDetailsDto>> GetSubjectForTeacher(string teacherEmail, int departmentId);
        Task<SubjectViewDto> AddSubjectAsync(SubjectCreateDto model);

        Task<SubjectViewDto> EditSubjectAsync(int id, SubjectUpdateDto model);
        Task<bool> DeleteSubjectAsync(int id);
    }
}
