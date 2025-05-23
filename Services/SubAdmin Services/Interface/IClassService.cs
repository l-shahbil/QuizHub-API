using QuizHub.Models.DTO.Class;
using QuizHub.Models.DTO.User.Student;

namespace QuizHub.Services.SubAdmin_Services.Interface
{
    public interface IClassService
    {
        Task<List<ClassViewDto>> GetAllClassesForSubAdminAsync(int departmentId, string subAdminEmail);
        Task<List<ClassViewDto>> GetAllClassesForTeacherAsync(int departmentId, string userId);
        Task<List<ClassViewDto>> GetAllClassesForStudentAsync(string userId);
        Task<ClassViewDto> EditClasssAsync(ClassUpdateDto model, int id, string subAdminEmail);

        Task<bool> DeleteClasssAsync(int id, string subAdminEmail);
        Task<ClassViewDto> AddClasssAsync(ClassCreateDto model, string subAdminEmail, int departmentId, int subjectId, string teacherEmail);

        Task<bool> AddStudentToClass(int departmentId, string subAdminEmail, int classId, string studentEmail);
        Task<bool> DeleteStudentFromClass(int departmentId, string subAdminEmail, int classId, string studentEmail);
        Task<bool> AddBatchToClass(int departmentId, string subAdminEmail, int classId, int batchId);
        Task<List<StudentViewDto>> GetAllStudentInClass(int departmentId, string userEmail, int classId);
    }
}
