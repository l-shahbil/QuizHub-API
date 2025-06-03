using QuizHub.Models.DTO.College;
using QuizHub.Models;
using QuizHub.Models.DTO.Department;
using QuizHub.Models.DTO.User.Teacher;
using QuizHub.Models.DTO.Subject;

namespace QuizHub.Services.Admin_Services.Interface
{
    public interface IDepartmentService
    {

        Task<List<DepartmentViewDto>> GetAllDepartmentsAsync();
        Task<DepartmentViewDto> GetDepartmentByIdAsync(int id);
        Task<List<DepartmentViewDto>> getDepartmentByCollegeId(string userEmail, int collegeId);
        Task<DepartmentViewDto> AddDepartmentAsync(DepartmentCreateDto model);

        Task<DepartmentViewDto> EditDepartmentAsync(int id, DepartmentUpdateDto model);
        Task<bool> DeleteDepartmentAsync(int id);
        Task<bool> AddTeacherToDepartmentAsync(int departmentId, List<string> teachersEmails);
        Task<bool> DeleteTeacherFromDepartmentAsync(int departmentId, string teacherEmail);
        Task<List<GetTeacherDto>> GetAllTeachersInDepartmentAsync( int departmentId);
        Task<bool> AddSubjectToDepartmentAsync(int departmentId, List<int> subjectIds);
        Task<bool> DeleteSubjectFromDepartmentAsync(int departmentId, int subjectId);
        Task<List<SubjectViewDto>> GetAllSubjectsInDepartmentAsync(int departmentId);

    }
}
