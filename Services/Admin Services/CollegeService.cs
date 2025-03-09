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
        public async Task<College> AddCollegeAsync(CreateCollegeDto model)
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

        public async Task<College> EditCollegeAsync(int id, UpdateCollegeDto model)
        {
            College college = await _collegeRepo.GetByIdAsync(id);
            if (college == null)
            {
                return null;
            }

            college.Name = model.Name ?? college.Name;
            college.Description = model.Description ?? college.Description;
            _collegeRepo.UpdateEntity(college);
            return college;
        }

        public async Task<List<GetCollegeDto>> GetAllCollegesAsync()
        {
            List<College> colleges = await _collegeRepo.GetAllAsync();

            var result = colleges.Select(c => new GetCollegeDto
            {
                Id = c.Id.ToString(),
                Name = c.Name,
                Description = c.Description,
            }).ToList();

            return result;
        }



        public async Task<GetCollegeIncludeDto> GetCollegeByIdAsync(int id)
        {
            College existingCollege = await _collegeRepo.GetByIdAsync<int>(id);
            if (existingCollege != null)
            {
                College college = await _collegeRepo.GetIncludeById(id, "Departments");
                return new GetCollegeIncludeDto()
                {
                    Id = college.Id.ToString(),
                    Name = college.Name,
                    Description = college.Description,
                    departmentName = college.Departments.Select(d => d.Name).ToList(),
                };
            }
            return null;
        }
    }
}
