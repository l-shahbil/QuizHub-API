using QuizHub.Models.DTO.Batch;
using QuizHub.Models.DTO.Class;
using QuizHub.Models.DTO.User.Student;

namespace QuizHub.Services.SubAdmin_Services.Interface
{
    public interface IBatchService
    {
        Task<List<BatchViewDto>> GetAllBathcesAsync(int departmentId, string subAdminEmail);
        Task<BatchViewDto> EditBatchAsync(BatchEditDto model, int id, string subAdminEmail);

        Task<bool> DeleteBatchAsync(int id, string subAdminEmail);
        Task<BatchViewDto> AddBatchAsync(BatchCreateDto model, string subAdminEmail, int departmentId);
        Task<BatchViewDetailsDto> GetBatchById(int id, int departmentId, string subAdminEmail);
        Task<List<StudentViewDto>> GetAllStudentInBatch(int departmentId, string subAdminEmail, int batchId);
        Task<bool> AddStudentToBatchAsync(int departmentId, string subAdminEmail, int batchId, string studentEmail);
        Task<bool> DeleteStudentFromBatchAsync(int departmentId, string subAdminEmail, int batchId, string studentEmail);
        Task<bool> AddListStudentsToBatchAsync(int departmentId, string subAdminEmail, int batchId, List<string> studenstEmails);
    }
}
