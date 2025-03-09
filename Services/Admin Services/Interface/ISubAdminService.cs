using QuizHub.Models;
using QuizHub.Models.DTO.User.SubAdmin;

namespace QuizHub.Services.Admin_Services.Interface
{
    public interface ISubAdminService
    {
        Task<IEnumerable<GetSubAdminDto>> GetAllSubAdmin();
        Task<GetSubAdminDto> GetSubAdminByNameAsync(string userName);
        Task<GetSubAdminDto> CreateSubAdminAsync(CreateSubAdminDto model);
        Task<GetSubAdminDto> EditSubAdminAsync(string userName, UpdateSubAdminDto model);
        Task<bool> DeleteSubAdminAsync(string userName);
    }
}
