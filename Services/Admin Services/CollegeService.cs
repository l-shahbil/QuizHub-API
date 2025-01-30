using Microsoft.AspNetCore.Http.HttpResults;
using QuizHub.Data.Repository.Base;
using QuizHub.Models;
using QuizHub.Models.DTO.College;
using System.Security.Policy;

namespace QuizHub.Services.Admin_Services.Interface
{
    public class CollegeService : ICollegeService
    {
        private readonly IRepository<College> _collegeRepo;
        public CollegeService(IRepository<College> collegeRepo)
        {
            _collegeRepo = collegeRepo;
        }
        public async Task<College> AddCollegeAsync(CollegeDto model)
        {
            var existingCollege = await _collegeRepo.SelecteOne(college => college.Name == model.Name);
            if (existingCollege != null)
            {
                return null;
            }

            College newCollege = new College()
            {
                Name = model.Name,
                Description = model.Description,
            };
            await _collegeRepo.AddAsyncEntity(newCollege);
            return newCollege;
        }

        public async Task<bool> DeleteCollegeAsync(int id)
        {
            College college = await _collegeRepo.GetByIdAsync(id);
            if (college == null)
            {
                return false;
            }

            _collegeRepo.DeleteEntity(college);
            return true;
        }

        public async Task<College> EditCollegeAsync(int id, CollegeUpdateDto model)
        {
            College college = await _collegeRepo.GetByIdAsync(id);
            if (college == null)
            {
                return null;
            }

            college.Name = model.Name;
            college.Description = model.Description;
            _collegeRepo.UpdateEntity(college);
            return college;
        }

        public async Task<List<College>> GetAllCollegesAsync()
        {
            List<College> colleges = await _collegeRepo.GetAllAsync();
            return colleges;
        }

        public async Task<College> GetCollegeByIdAsync(int id)
        {
            College existingCollege = await _collegeRepo.GetByIdAsync<int>(id);
            if (existingCollege != null)
            {
                return await _collegeRepo.GetIncludeById(id, "Departments");
            }
            return null;
        }
    }
}
