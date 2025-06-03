using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using QuizHub.Constant;
using QuizHub.Data.Repository.Base;
using QuizHub.Models;
using QuizHub.Models.DTO.College;
using QuizHub.Utils.Interface;
using System.Security.Policy;

namespace QuizHub.Services.Admin_Services.Interface
{
    public class CollegeService : ICollegeService
    {
        private readonly IRepository<College> _collegeRepo;
        private readonly UserManager<AppUser> _userManager;
        private readonly IRepository<Department> _departmentRepo;
        private readonly IRepository<UserDepartment> _userDepartmentRepo;
        private readonly IDeleteService _deleteService;

        public CollegeService(IRepository<College> collegeRepo,UserManager<AppUser> userManger,IRepository<Department> departmentRepo,
            IRepository<UserDepartment> userDepartmentRepo,IDeleteService deleteService)
        {
            _collegeRepo = collegeRepo;
            _userManager = userManger;
            _departmentRepo = departmentRepo;
            _userDepartmentRepo = userDepartmentRepo;
            _deleteService = deleteService;
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
            College college = await _collegeRepo.GetIncludeById(id, "Departments");
            if (college == null)
            {
                return false;
            }

            foreach (Department dept in college.Departments)
            {
                await _deleteService.deleteDepartment(dept);
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
            var existColleage = await _collegeRepo.SelecteOne(c => c.Name == model.Name);
            if (existColleage != null)
            {
                throw new ArgumentException("A colleage with the same name already exists.");
            }
            college.Name = string.IsNullOrWhiteSpace(model.Name) ? college.Name:model.Name;
            college.Description =string.IsNullOrWhiteSpace(model.Description)? college.Description: model.Description;
            _collegeRepo.UpdateEntity(college);
            return college;
        }

        public async Task<List<GetCollegeDto>> GetAllCollegesAsync(string userEmail)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            var roles = await _userManager.GetRolesAsync(user);
            List<GetCollegeDto> result = new List<GetCollegeDto>();
            List<College> colleges = await _collegeRepo.GetAllAsync();

            if (roles.Contains(Roles.Admin.ToString()))
            {

                result = colleges.Select(c => new GetCollegeDto
                {
                    Id = c.Id.ToString(),
                    Name = c.Name,
                    Description = c.Description,
                }).ToList();
            }
            if (roles.Contains(Roles.SubAdmin.ToString()))
            {
               List<Department> allDepartment =await _departmentRepo.GetAllAsync();
                List<Department> departmentsForSubAdmin = allDepartment.Where(d=> d.subAdminId == user.Id).ToList();
                List<int> collegeIds = departmentsForSubAdmin.Select(d=> d.collegeId).ToList();

                List<College> collegesForSubAdmin = colleges.Where(c => collegeIds.Contains(c.Id)).ToList();
                return collegesForSubAdmin.Select(c=> new GetCollegeDto
                {
                    Id = c.Id.ToString(),
                    Name = c.Name,
                    Description = c.Description,

                }).ToList();
            }
            if (roles.Contains(Roles.Teacher.ToString()))
            {
                List<UserDepartment> allDepartmentUser = await _userDepartmentRepo.GetAllIncludeAsync("Department");
                List<Department> allDepartmentForTeacher = allDepartmentUser.Where(ud=> ud.userId == user.Id).Select(ud=> ud.Department).ToList();
                List<int> collegeIds = allDepartmentForTeacher.Select(d=> d.collegeId).ToList();

                List<College> collegesForTeacher = colleges.Where(c => collegeIds.Contains(c.Id)).ToList();

                return collegesForTeacher.Select(c => new GetCollegeDto
                {
                    Id = c.Id.ToString(),
                    Name = c.Name,
                    Description = c.Description,

                }).ToList();
            }

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
