
using Microsoft.AspNetCore.Identity;
using QuizHub.Data.Repository.Base;
using QuizHub.Models;
using QuizHub.Models.DTO.User.Student;

namespace QuizHub.Services.SubAdmin_Services.Interface
{
    public interface IStudentService
    {
    Task<StudentViewDetailsDto> CreateStudentAsync(StudentCreateDto model, string subAdminEmail, int departmentId)
; Task<StudentViewDetailsDto> EditStudentAsync(string userName, StudentEditDto model, string subAdminEmail, int departmentId)
; Task<bool> DeleteStudentAsync(string userName, string subAdminEmail, int departmentId)
; Task<IEnumerable<StudentViewDto>> GetAllStudent(int departmentId, string subAdminEmail)
; Task<StudentViewDetailsDto> GetStudentByNameAsync(int departmentId, string userName, string subAdminEmail);
            Task<int> GetStudentsCounts();

    }
}
