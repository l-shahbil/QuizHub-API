using Microsoft.AspNetCore.Identity;
using QuizHub.Constant;
using QuizHub.Data.Repository.Base;
using QuizHub.Models;
using QuizHub.Models.DTO.Batch;
using QuizHub.Models.DTO.Class;
using QuizHub.Models.DTO.User.Student;
using QuizHub.Services.SubAdmin_Services.Interface;
using QuizHub.Utils.Interface;

namespace QuizHub.Services.SubAdmin_Services
{
    public class BatchService :IBatchService
    {
        private readonly IRepository<Batch> _batchRepo;
        private readonly IRepository<Department> _departmentRepo;
        private readonly IRepository<AppUser> _studentRepo;
        private readonly IRepository<UserDepartment> _userDepartmentRepo;
        private readonly UserManager<AppUser> _userManager;
        private readonly IDeleteService _deleteService;

        public BatchService(IRepository<Batch> batchRepo,IRepository<Department> departmentRepo,IRepository<AppUser> studentRepo,
            IRepository<UserDepartment> userDepartmentRepo,UserManager<AppUser> userManager,IDeleteService deleteService)
        {
            _batchRepo = batchRepo;
            _departmentRepo = departmentRepo;
            _studentRepo = studentRepo;
            _userDepartmentRepo = userDepartmentRepo;
            _userManager = userManager;
            _deleteService = deleteService;
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

            Batch batch = await _batchRepo.GetIncludeById(id);
            if (batch == null)
            {
                throw new KeyNotFoundException($"Batch with ID {id} not found.");
            }

            Department department = await _departmentRepo.GetIncludeById(batch.DepartmentId, "SubAdmin");
            if (department.SubAdmin.Email != subAdminEmail)
            {
                throw new UnauthorizedAccessException("Access Denied: You are not the assigned SubAdmin for this department.");
            }

            await _deleteService.deleteBatch(batch);
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
        

        public async Task<BatchViewDetailsDto> GetBatchById(int id,int departmentId,string subAdminEmail)
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
            Batch batch = await _batchRepo.GetIncludeById(id, "Students");
            if (batch == null || batch.DepartmentId != departmentId)
            {
                throw new KeyNotFoundException($"Batch with ID {id} not found.");
            }

            return new BatchViewDetailsDto
            {
                Id = batch.Id,
                Name = batch.Name,
                Description = batch.Description,
                Students = batch.Students.Select(s => new StudentViewDto
                {
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    Email = s.Email
                }).ToList()
            };
        }
       public async Task<List<StudentViewDto>> GetAllStudentInBatch(int departmentId, string subAdminEmail, int batchId)
        {
            var department = await _departmentRepo.GetIncludeById(departmentId, "SubAdmin");
            if (department == null)
            {
                throw new ArgumentException("Department not found.");
            }
            if (department.SubAdmin.Email != subAdminEmail)
            {
                throw new UnauthorizedAccessException("Access Denied: You are not the assigned SubAdmin for this department.");
            }
            var batch = await _batchRepo.GetIncludeById(batchId, "Students");
            if (batch == null || batch.DepartmentId != departmentId)
            {
                throw new ArgumentException("Batch not found.");

            }
            return batch.Students.Select(s => new StudentViewDto
            {
                FirstName = s.FirstName,
                LastName = s.LastName,
                Email = s.Email
            }).ToList();
        }
        public async Task<bool> AddStudentToBatchAsync(int departmentId, string subAdminEmail, int batchId, List<string> studenstEmails)
        {
            var department = await _departmentRepo.GetIncludeById(departmentId, "SubAdmin");
            if (department == null)
            {
                throw new KeyNotFoundException("Department not found.");
            }

            if (department.SubAdmin.Email != subAdminEmail)
            {
                throw new UnauthorizedAccessException("Access Denied: You are not the assigned SubAdmin for this department.");
            }

            var batch = await _batchRepo.GetIncludeById(batchId, "Students");
            if (batch == null || batch.DepartmentId != departmentId)
            {
                throw new KeyNotFoundException("Batch not found.");
            }

            foreach (string email in studenstEmails)
            {

                var student = await _studentRepo.SelecteOne(s => s.Email == email);
                var isStudentInDepartment = (await _userDepartmentRepo.GetAllAsync())
                    .FirstOrDefault(s => s.userId == student.Id && s.departmentId == departmentId);

                var isStudentRole = await _userManager.IsInRoleAsync(student, Roles.Student.ToString());

                if (student == null || isStudentInDepartment == null || !isStudentRole)
                {
                    throw new KeyNotFoundException($"{email} not found or not assigned to the department.");
                }

                if (batch.Students.Contains(student))
                {
                    throw new InvalidOperationException($"{email} already exists in the batch.");
                }

                student.Batch = batch;
                _studentRepo.UpdateEntity(student);

            }
            return true;
        }

   
        public async Task<bool> DeleteStudentFromBatchAsync(int departmentId,string subAdminEmail,int batchId, string studentEmail)
        {
            var department = await _departmentRepo.GetIncludeById(departmentId, "SubAdmin");
            if (department == null)
            {
                throw new ArgumentException("Department not found.");
            }
            if (department.SubAdmin.Email != subAdminEmail)
            {
                throw new UnauthorizedAccessException("Access Denied: You are not the assigned SubAdmin for this department.");
            }
            var batch = await _batchRepo.GetIncludeById(batchId, "Students");
            if (batch == null || batch.DepartmentId != departmentId)
            {
                throw new ArgumentException("Batch not found.");

            }
           
            var student = await _studentRepo.SelecteOne(s => s.Email == studentEmail);
            if (student == null || student.BatchId != batch.Id)
            {
                throw new ArgumentException($"{studentEmail} not found.");
            }

            student.Batch = null;

            _studentRepo.UpdateEntity(student);
            

            return true;
        }
    }
}
