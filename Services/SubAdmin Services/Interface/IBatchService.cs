using QuizHub.Models.DTO.Batch;
using QuizHub.Models.DTO.Class;

namespace QuizHub.Services.SubAdmin_Services.Interface
{
    public interface IBatchService
    {
        Task<List<BatchViewDto>> GetAllBathcesAsync(int departmentId, string subAdminEmail);
        Task<BatchViewDto> EditBatchAsync(BatchEditDto model, int id, string subAdminEmail);

        Task<bool> DeleteBatchAsync(int id, string subAdminEmail);
        Task<BatchViewDto> AddBatchAsync(BatchCreateDto model, string subAdminEmail, int departmentId);
        Task<BatchViewDetailsDto> GetBatchById(int id,string subAdminEmail);
    }
}
