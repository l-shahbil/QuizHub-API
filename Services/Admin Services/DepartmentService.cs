
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuizHub.Constant;
using QuizHub.Data.Repository.Base;
using QuizHub.Models;
using QuizHub.Models.DTO.Department;
using QuizHub.Models.DTO.Subject;
using QuizHub.Models.DTO.User.Teacher;
using System.Security.Policy;

namespace QuizHub.Services.Admin_Services.Interface
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IRepository<Department> _departmentRepo;
        private readonly IRepository<UserDepartment> _userDepartmentRepo;
        private readonly IRepository<College> _collegeRepo;
        private readonly UserManager<AppUser> _userManager;
        private readonly IRepository<UserDepartment> _userDepartment;
        private readonly IRepository<Subject> _subjectRepo;

        public DepartmentService(IRepository<Department> departmentRepo,IRepository<UserDepartment> userDepartmentRepo, IRepository<College> collegeRepo, UserManager<AppUser> userManager, IRepository<UserDepartment> userDepartment, IRepository<Subject> subjectRepo)
        {
            _departmentRepo = departmentRepo;
            _userDepartmentRepo = userDepartmentRepo;
            _collegeRepo = collegeRepo;
            _userManager = userManager;
            _userDepartment = userDepartment;
            _subjectRepo = subjectRepo;
        }
        public async Task<List<DepartmentViewDto>> GetAllDepartmentsAsync()
        {
            List<Department> departments = await _departmentRepo.GetAllIncludeAsync("Colleges", "SubAdmin");
            List<DepartmentViewDto> departmentViewDtos = new List<DepartmentViewDto>();

            foreach (Department department in departments)
            {
                departmentViewDtos.Add(new DepartmentViewDto
                {
                    Id = department.Id,
                    Name = department.Name,
                    Description = department.Description,
                    CollegeName = department.Colleges.Name,
                    SubAdmin = department.SubAdmin.Email

                });
            }

            return departmentViewDtos;
        }

        public async Task<DepartmentViewDto> GetDepartmentByIdAsync(int id)
        {
            Department existingDepartment = await _departmentRepo.GetByIdAsync<int>(id);
            if (existingDepartment != null)
            {
                var department = _departmentRepo.GetIncludeById(id, "SubAdmin", "Colleges").Result;
                var departmentWithSubAdmin = new DepartmentViewDto()
                {
                    Id = department.Id,
                    Name = department.Name,
                    Description = department.Description,
                    SubAdmin = department.SubAdmin.Email,
                    CollegeName = department.Colleges.Name,
                };
                return departmentWithSubAdmin;

            }
            return null;
        }
        public async Task<List<DepartmentViewDto>> getDepartmentByCollegeId(string userEmail,int collegeId)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            var roles = await _userManager.GetRolesAsync(user);
            College college = await _collegeRepo.GetIncludeById(collegeId, "Departments");
            if (college == null)
            {
                throw new ArgumentException("College not found");
            }

            if (roles.Contains(Roles.Admin.ToString()))
            {
            return college.Departments.Select(d=> new DepartmentViewDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                CollegeName= d.Colleges.Name,
            }).ToList();
            }
            else if (roles.Contains(Roles.SubAdmin.ToString()))
            {
                return college.Departments.Where(d=> d.subAdminId == user.Id).Select(d => new DepartmentViewDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    CollegeName = d.Colleges.Name,
                }).ToList();
            }
            else if (roles.Contains(Roles.Teacher.ToString()))
            {
                List<UserDepartment> allDepartmentUser = await _userDepartmentRepo.GetAllIncludeAsync("Department");
                List<Department> allDepartmentForTeacher = allDepartmentUser.Where(ud => ud.userId == user.Id && ud.Department.collegeId == collegeId)
                    .Select(ud => ud.Department).ToList();

                return allDepartmentForTeacher.Select(d => new DepartmentViewDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    CollegeName = d.Colleges.Name,
                }).ToList();

            }
            return null;
        }
        public async Task<DepartmentViewDto> AddDepartmentAsync(DepartmentCreateDto model)
        {
            var existingDepartment = await _departmentRepo.SelecteOne(Department => Department.Name == model.Name);
            if (existingDepartment != null)
            {
                throw new ArgumentException("A department with the same name already exists.");
            }

            var college = await _collegeRepo.GetByIdAsync(model.collegeId);
            if (college == null)
            {
                throw new ArgumentException("College not found");
            }
            var subAdmin = await _userManager.FindByEmailAsync(model.userName);
            if (subAdmin == null)
            {
                throw new ArgumentException("SubAdmin not found");
            }
            Department newDepartment = new Department()
            {
                Name = model.Name,
                Description = model.Description,
                Colleges = college,
                SubAdmin = subAdmin
            };

            await _departmentRepo.AddAsyncEntity(newDepartment);


            return new DepartmentViewDto()
            {
                Id = newDepartment.Id,
                Name = newDepartment.Name,
                Description = newDepartment.Description,
                CollegeName = college.Name,
                SubAdmin = subAdmin.Email
            };
        }

        public async Task<bool> DeleteDepartmentAsync(int id)
        {
            Department Department = await _departmentRepo.GetByIdAsync(id);
            if (Department == null)
            {
                return false;
            }

            _departmentRepo.DeleteEntity(Department);
            return true;
        }
        public async Task<DepartmentViewDto> EditDepartmentAsync(int id, DepartmentUpdateDto model)
        {
            var department = await _departmentRepo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Department with ID {id} not found.");
            var existingDepartment = await _departmentRepo.SelecteOne(Department => Department.Name == model.Name);
            if (existingDepartment != null)
            {
                throw new ArgumentException("A department with the same name already exists.");
            }
            if (model.CollegeId.HasValue)
            {
                var college = await _collegeRepo.GetByIdAsync(model.CollegeId)
                    ?? throw new KeyNotFoundException($"College with ID {model.CollegeId} not found.");
                department.Colleges = college;
            }


            if (!string.IsNullOrWhiteSpace(model.SubAdmin))
            {
                var subAdmin = await _userManager.FindByEmailAsync(model.SubAdmin)
                    ?? throw new ArgumentException("SubAdmin not found.", nameof(model.SubAdmin));
                department.SubAdmin = subAdmin;
            }

            department.Name = model.Name ?? department.Name;
            department.Description = model.Description ?? department.Description;

            _departmentRepo.UpdateEntity(department);

            return new DepartmentViewDto
            {
                Id = department.Id,
                Name = department.Name,
                Description = department.Description,
                CollegeName = department.Colleges?.Name,
                SubAdmin = department.SubAdmin?.Email
            };
        }



        public async Task<bool> AddTeacherToDepartmentAsync(int departmentId, string teacherEmail)
        {
            var department = await _departmentRepo.GetByIdAsync(departmentId);
            if (department == null)
            {
                throw new ArgumentException("Department not found.");
            }

            var teacher = await _userManager.FindByEmailAsync(teacherEmail);
            if (teacher == null)
            {
                throw new ArgumentException("Teacher not found.");
            }

            var existingRelation = await _userDepartment.SelecteOne(ud => ud.userId == teacher.Id && ud.departmentId == departmentId);
            if (existingRelation != null)
            {
                throw new ArgumentException("Teacher is already assigned to this department.");
            }

            var userDepartment = new UserDepartment
            {
                userId = teacher.Id,
                departmentId = departmentId
            };

            await _userDepartment.AddAsyncEntity(userDepartment);

            return true;
        }

        public async Task<List<GetTeacherDto>> GetAllTeachersInDepartmentAsync(int departmentId)
        {
         

            var departmentExists = await _departmentRepo.GetByIdAsync(departmentId);
            if (departmentExists == null)
            {
                throw new KeyNotFoundException("Department not found.");
            }

         
            var userDepartment = await _userDepartment.GetAllIncludeAsync("User");

            var filteredTeachers = userDepartment
                .Where(ud => ud.departmentId == departmentId && _userManager.IsInRoleAsync(ud.User, Roles.Teacher.ToString()).Result)
                .Select(ud => new GetTeacherDto
                {
                    Email = ud.User.Email,
                    FirstName = ud.User.FirstName,
                    LastName = ud.User.LastName,
                    RegistraionDate = ud.User.RegistraionDate,
                    DateOfBirth = ud.User.DateOfBirth
                })
                .ToList();

            return filteredTeachers;
        }

       public async Task<bool> DeleteTeacherFromDepartmentAsync(int departmentId, string teacherEmail)
        {

            var department = await _departmentRepo.GetByIdAsync(departmentId);
            if (department == null)
            {
                throw new ArgumentException("Department not found.");
            }

            var teacher = await _userManager.FindByEmailAsync(teacherEmail);
            if (teacher == null)
            {
                throw new ArgumentException("Teacher not found.");
            }

            var existingRelation = await _userDepartment.SelecteOne(ud => ud.userId == teacher.Id && ud.departmentId == departmentId);
            if (existingRelation == null)
            {
                throw new ArgumentException("Teacher is not assigned to this department.");
            }


            _userDepartment.DeleteEntity(existingRelation);

            return true;
        }

        public async Task<bool> AddSubjectToDepartmentAsync(int departmentId, int subjectId)
        {
            var department = await _departmentRepo.GetIncludeById(departmentId, "Subjects");
            if (department == null)
            {
                throw new ArgumentException("Department not found.");
            }

            var subject = await _subjectRepo.GetByIdAsync(subjectId);
            if (subject == null)
            {
                throw new ArgumentException("Subject not found.");
            }

            var existingRelation = department.Subjects.Any(s => s.Id == subjectId);
            if (existingRelation)
            {
                throw new ArgumentException("Subject is already assigned to this department.");
            }

            department.Subjects.Add(subject);
            _departmentRepo.UpdateEntity(department);

            return true;
        }

        public async Task<bool> DeleteSubjectFromDepartmentAsync(int departmentId, int subjectId)
        {
            var department = await _departmentRepo.GetIncludeById(departmentId, "Subjects");
            if (department == null)
            {
                throw new ArgumentException("Department not found.");
            }

            var subject = await _subjectRepo.GetByIdAsync(subjectId);
            if (subject == null)
            {
                throw new ArgumentException("Subject not found.");
            }

            var existingRelation = department.Subjects.Any(s => s.Id == subjectId);
            if (!existingRelation)
            {
                throw new ArgumentException("Subject is not assigned to this department.");
            }


            department.Subjects.Remove(subject);
            _departmentRepo.UpdateEntity(department);

            return true;
        }

        public async Task<List<SubjectViewDto>> GetAllSubjectsInDepartmentAsync(int departmentId)
        {
            var departmentExists = await _departmentRepo.GetIncludeById(departmentId, "Subjects");
            if (departmentExists == null)
            {
                throw new Exception("Department not found.");
            }

            var filteredSubjects= departmentExists.Subjects
                .Select(s => new SubjectViewDto
                {
                   Id =s.Id,
                   Name = s.Name,
                   Description = s.Description,
                })
                .ToList();

            return filteredSubjects;
        }
    }
    }
