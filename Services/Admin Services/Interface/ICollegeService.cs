using QuizHub.Models.DTO.College;
using QuizHub.Models;

namespace QuizHub.Services.Admin_Services.Interface
{
    public interface ICollegeService
    {
        Task<List<College>> GetAllCollegesAsync();
        Task<College> GetCollegeByIdAsync(int id);
        Task<College> AddCollegeAsync(CollegeDto model);
        Task<College> EditCollegeAsync(int id, CollegeUpdateDto model);
        Task<bool> DeleteCollegeAsync(int id);
    }
}
