using QuizHub.Models.DTO.College;
using QuizHub.Models;

namespace QuizHub.Services.Admin_Services.Interface
{
    public interface ICollegeService
    {
        Task<List<GetCollegeDto>> GetAllCollegesAsync();
        Task<GetCollegeIncludeDto> GetCollegeByIdAsync(int id);
        Task<College> AddCollegeAsync(CreateCollegeDto model);
        Task<College> EditCollegeAsync(int id, UpdateCollegeDto model);
        Task<bool> DeleteCollegeAsync(int id);
    }
}
