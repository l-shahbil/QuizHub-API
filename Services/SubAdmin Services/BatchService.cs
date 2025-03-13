using QuizHub.Data.Repository.Base;
using QuizHub.Models;
using QuizHub.Models.DTO.Batch;
using QuizHub.Models.DTO.Class;
using QuizHub.Services.SubAdmin_Services.Interface;

namespace QuizHub.Services.SubAdmin_Services
{
    public class BatchService : IBatchService
    {
        private readonly IRepository<Batch> _batchRepo;
        private readonly IRepository<Department> _departmentRepo;

        public BatchService(IRepository<Batch> batchRepo,IRepository<Department> departmentRepo)
        {
            _batchRepo = batchRepo;
            _departmentRepo = departmentRepo;
        }

        public async Task<BatchViewDto> AddBatchAsync(BatchCreateDto model, string subAdminEmail, int departmentId)
        {
            var existDepartment = await _departmentRepo.GetIncludeById(departmentId, "SubAdmin");

            if (existDepartment == null)
            {
                throw new KeyNotFoundException($"A Department with ID {departmentId} not found.");
            }
            if (existDepartment.SubAdmin.Email != subAdminEmail)
            {
                throw new UnauthorizedAccessException("Access Denied: You are not the assigned SubAdmin for this department.");
            }


            var existBatch = await _batchRepo.SelecteOne(c => c.Name == model.Name);
            if (existBatch != null)
            {
                throw new ArgumentException("A Batch with the same name already exists.");
            }

            Batch batch = new Batch()
            {
                Name = model.Name,
                Description = model.Description
                ,Department = existDepartment
            };
            await _batchRepo.AddAsyncEntity(batch);
            return new BatchViewDto()
            {
                Id = batch.Id,
                Name = batch.Name,
                Description = batch.Description
            };
        }

        public async Task<bool> DeleteBatchAsync(int id, string subAdminEmail)
        {

            Batch batch = await _batchRepo.GetByIdAsync(id);
            if (batch == null)
            {
                throw new KeyNotFoundException($"Batch with ID {id} not found.");
            }

            Department department = await _departmentRepo.GetIncludeById(batch.DepartmentId, "SubAdmin");
            if (department.SubAdmin.Email != subAdminEmail)
            {
                throw new UnauthorizedAccessException("Access Denied: You are not the assigned SubAdmin for this department.");
            }

            _batchRepo.DeleteEntity(batch);
            return true;
        }

        public async Task<BatchViewDto> EditBatchAsync(BatchEditDto model, int id, string subAdminEmail)
        {
            var batch = await _batchRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Batch with ID {id} not found.");
            Department department = await _departmentRepo.GetIncludeById(batch.DepartmentId, "SubAdmin");
            if (department.SubAdmin.Email != subAdminEmail)
            {
                throw new UnauthorizedAccessException("Access Denied: You are not the assigned SubAdmin for this department.");
            }

            batch.Name = string.IsNullOrWhiteSpace(model.Name) ? batch.Name : model.Name;
            batch.Description = string.IsNullOrWhiteSpace(model.Description) ? batch.Description : model.Description;

            _batchRepo.UpdateEntity(batch);
            return new BatchViewDto()
            {
                Id = batch.Id,
                Name = batch.Name,
                Description = batch.Description
            };
        }

        public async Task<List<BatchViewDto>> GetAllBathcesAsync(int departmentId, string subAdminEmail)
        {
            var department = await _departmentRepo.GetIncludeById(departmentId, "SubAdmin");
            if (department == null)
            {
                throw new KeyNotFoundException($"A Department with ID {departmentId} not found.");
            }
            if (department.SubAdmin.Email != subAdminEmail)
            {
                throw new UnauthorizedAccessException("Access Denied: You are not the assigned SubAdmin for this department.");
            }

            List<Batch> batches = await _batchRepo.GetAllIncludeAsync("Department");
            var filteredBatches = batches
       .Where(b => b.Department.Id == department.Id)
       .Select(b => new BatchViewDto
       {
           Id = b.Id,
           Name = b.Name,
           Description = b.Description
       })
       .ToList();

            return filteredBatches;
        }
        

        public async Task<BatchViewDetailsDto> GetBatchById(int id,string subAdminEmail)
        {

            Batch batch = await _batchRepo.GetIncludeById(id, "Students");
            if (batch == null)
            {
                throw new KeyNotFoundException($"Batch with ID {id} not found.");
            }

            Department department = await _departmentRepo.GetIncludeById(batch.DepartmentId, "SubAdmin");
            if (department.SubAdmin.Email != subAdminEmail)
            {
                throw new UnauthorizedAccessException("Access Denied: You are not the assigned SubAdmin for this department.");
            }

            return new BatchViewDetailsDto
            {
                Id = batch.Id,
                Name = batch.Name,
                Description = batch.Description,
                Students = batch.Students.ToList()
            };
        }
    }
}
