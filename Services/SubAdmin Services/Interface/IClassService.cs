using QuizHub.Models.DTO.Class;

namespace QuizHub.Services.SubAdmin_Services.Interface
{
    public interface IClassService
    {
        Task<List<ClassViewDto>> GetAllClassesForSubAdminAsync(int departmentId, string subAdminEmail);
        Task<List<ClassViewDto>> GetAllClassesForTeacherAsync(string userId);
        Task<List<ClassViewDto>> GetAllClassesForStudentAsync(string userId);
        Task<ClassViewDto> EditClasssAsync(ClassUpdateDto model, int id, string subAdminEmail);

        Task<bool> DeleteClasssAsync(int id, string subAdminEmail);
        Task<ClassViewDto> AddClasssAsync(ClassCreateDto model, string subAdminEmail, int departmentId, int subjectId, string teacherEmail);
    }
}
