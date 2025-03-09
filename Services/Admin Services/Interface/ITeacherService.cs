using QuizHub.Models.DTO.User.Teacher;

namespace QuizHub.Services.Admin_Services.Interface
{
    public interface ITeacherService
    {
        Task<IEnumerable<GetTeacherDto>> GetAllTeacher();
        Task<GetTeacherDto> GetTeacherByNameAsync(string userName);
        Task<GetTeacherDto> CreateTeacherAsync(CreateTeacherDto model);
        Task<GetTeacherDto> EditTeacherAsync(string userName, UpdateTeacherDto model);
        Task<bool> DeleteTeacherAsync(string userName);
        

    }
}
