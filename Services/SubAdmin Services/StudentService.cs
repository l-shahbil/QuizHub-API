using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Writers;
using NuGet.DependencyResolver;
using QuizHub.Constant;
using QuizHub.Data.Repository.Base;
using QuizHub.Models;
using QuizHub.Models.DTO.User.Student;
using QuizHub.Models.DTO.User.SubAdmin;
using QuizHub.Models.DTO.User.Teacher;
using QuizHub.Services.SubAdmin_Services.Interface;
using QuizHub.Utils;
using QuizHub.Utils.Interface;
using System.Linq;
using System.Text.RegularExpressions;

namespace QuizHub.Services.SubAdmin_Services
{
    public class StudentService : IStudentService
    {
        private readonly IRepository<AppUser> _studentRepo;
        private readonly IRepository<Department> _departmentRepo;
        private readonly UserManager<AppUser> _userManager;
        private readonly IRepository<Batch> _batchRepo;
        private readonly IDeleteService _deleteServices;
        private readonly IRepository<UserDepartment> _userDepartmentRepo;
        private readonly IRepository<StudentAnswers> _studentAnswerRepo;
        private readonly IRepository<StudentExam> _studentExamRepo;
        private readonly IRepository<StudentClass> _studentClassRepo;

        public StudentService(IRepository<AppUser>studentRepo,IRepository<Department> departmentRepo,UserManager<AppUser> userManager,IRepository<Batch> batchRepo,IDeleteService deleteServices
            ,IRepository<UserDepartment> userDepartmentRepo,IRepository<StudentAnswers> studentAnswerRepo,IRepository<StudentExam>studentExamRepo,IRepository<StudentClass> studentClassRepo)
        {
            _studentRepo = studentRepo;
            _departmentRepo = departmentRepo;
            _userManager = userManager;
            _batchRepo = batchRepo;
            _deleteServices = deleteServices;
            _userDepartmentRepo = userDepartmentRepo;
            _studentAnswerRepo = studentAnswerRepo;
            _studentExamRepo = studentExamRepo;
            _studentClassRepo = studentClassRepo;
        }
        public async Task<StudentViewDetailsDto> CreateStudentAsync(StudentCreateDto model, string subAdminEmail,int departmentId)
        {
            var department = await _departmentRepo.GetIncludeById(departmentId, "SubAdmin");
            if (department == null)
            {
                throw new ArgumentException($"A Department with ID {departmentId} not found.");
            }
            if (department.SubAdmin.Email != subAdminEmail)
            {
                throw new UnauthorizedAccessException("Access Denied: You are not the assigned SubAdmin for this department.");
            }


            var existingStudent = await _userManager.FindByEmailAsync(model.Email);
            if (existingStudent != null)
            {
                throw new ArgumentException("The email address is already registered.");
            }
            if (model.DateOfBirth > DateTime.UtcNow)
            {
                throw new ArgumentException("Date of birth cannot be in the future.");
            }

            var newStudent = new AppUser
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.Email,
                RegistraionDate = DateTime.UtcNow,
                DateOfBirth = model.DateOfBirth,
            };

            var createResult = await _userManager.CreateAsync(newStudent, model.PassWord);
            if (!createResult.Succeeded)
            {
                throw new Exception($"Failed to create SubAdmin: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }

            var roleResult = await _userManager.AddToRoleAsync(newStudent, Roles.Student.ToString());
            if (!roleResult.Succeeded)
            {
                throw new Exception($"Failed to assign role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }

            UserDepartment studentDepartment = new UserDepartment()
            {
                User = newStudent,
                Department = department
            };

            try
            {
                await _userDepartmentRepo.AddAsyncEntity(studentDepartment);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to add the student to the department.", ex);
            }

            return new StudentViewDetailsDto()
            {
                Email = newStudent.Email,
                FirstName = newStudent.FirstName,
                LastName = newStudent.LastName,
                RegistraionDate = newStudent.RegistraionDate,
                DateOfBirth = newStudent.DateOfBirth,
            };
        }

        public async Task<bool> DeleteStudentAsync(string userName, string subAdminEmail, int departmentId)
        {
            var department = await _departmentRepo.GetIncludeById(departmentId, "SubAdmin");
            if (department == null)
            {
                throw new ArgumentException($"A Department with ID {departmentId} not found.");
            }
            if (department.SubAdmin.Email != subAdminEmail)
            {
                throw new UnauthorizedAccessException("Access Denied: You are not the assigned SubAdmin for this department.");
            }

            AppUser user = await _userManager.FindByEmailAsync(userName);
            var IsStudentInDepartment =_userDepartmentRepo.GetAllAsync().Result.FirstOrDefault(ud=> ud.userId == user.Id && ud.departmentId == department.Id);
            if (user == null || IsStudentInDepartment ==null || !_userManager.IsInRoleAsync(user, Roles.Student.ToString()).Result)
            {
                throw new ArgumentException($"Student with email {userName} not found.");

            }

            await _deleteServices.deleteSudent(user);
            return true;
        }

        public async Task<StudentViewDetailsDto> EditStudentAsync(string userName, StudentEditDto model, string subAdminEmail, int departmentId)
        {
            var department = await _departmentRepo.GetIncludeById(departmentId, "SubAdmin");
            if (department == null)
            {
                throw new ArgumentException($"A Department with ID {departmentId} not found.");
            }
            if (department.SubAdmin.Email != subAdminEmail)
            {
                throw new UnauthorizedAccessException("Access Denied: You are not the assigned SubAdmin for this department.");
            }

            var student = await _userManager.FindByEmailAsync(userName);
            if (student == null)
            {
                throw new ArgumentException($"User with email {userName} not found.");

            }

            if (!string.IsNullOrEmpty(model.Email))
            {
                string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                if (!Regex.IsMatch(model.Email, emailPattern, RegexOptions.IgnoreCase))
                {
                    throw new ArgumentException("Invalid email address.");
                }


                var newStudent = await _userManager.FindByEmailAsync(model.Email);
                if (newStudent != null)
                {
                    throw new ArgumentException("Username already exists.");
                }
                student.Email = model.Email;
                student.UserName = model.Email;
            }

            student.FirstName = string.IsNullOrWhiteSpace(model.FirstName) ? student.FirstName : model.FirstName;
            student.LastName = string.IsNullOrWhiteSpace(model.LastName) ? student.LastName : model.LastName;
            if (model.DateOfBirth.HasValue && model.DateOfBirth > DateTime.UtcNow)
            {
                student.DateOfBirth = model.DateOfBirth.Value;
            }



            if (!string.IsNullOrWhiteSpace(model.PassWord))
            {
                if (model.PassWord.Length < 7)
                {
                    throw new ArgumentException("Password does not meet the requirements.");

                }
                var passwordChangeResult = await _userManager.RemovePasswordAsync(student);
                if (!passwordChangeResult.Succeeded)
                {
                    throw new ArgumentException("Password change failed.");
                }

                var addPasswordResult = await _userManager.AddPasswordAsync(student, model.PassWord);
                if (!addPasswordResult.Succeeded)
                {
                    throw new ArgumentException("Password change failed.");
                }
            }

            var updateResult = await _userManager.UpdateAsync(student);
            return updateResult.Succeeded ? new StudentViewDetailsDto()
            {
                Email = student.Email,
                FirstName = student.FirstName,
                LastName = student.LastName,
                RegistraionDate = student.RegistraionDate,
                DateOfBirth = student.DateOfBirth
            } : null;
        }
        

        public async Task<IEnumerable<StudentViewDto>> GetAllStudent(int departmentId,string subAdminEmail)
        {
            var department = await _departmentRepo.GetIncludeById(departmentId, "SubAdmin");
            if (department == null)
            {
                throw new ArgumentException($"A Department with ID {departmentId} not found.");
            }
            if (department.SubAdmin.Email != subAdminEmail)
            {
                throw new UnauthorizedAccessException("Access Denied: You are not the assigned SubAdmin for this department.");
            }
            var users = _userDepartmentRepo.GetAllAsync().Result.Where(d => d.departmentId ==departmentId).Select(u => u.userId);
            var filteredStudents = _studentRepo.GetAllAsync().Result
                .Where( u => users.Contains(u.Id) && _userManager.IsInRoleAsync(u, Roles.Student.ToString()).Result)
                .Select(x => new StudentViewDto
                {
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Email = x.Email
                });

            return filteredStudents;
        }

        public async Task<StudentViewDetailsDto> GetStudentByNameAsync(int departmentId, string userName,string subAdminEmail)
        {
            var department = await _departmentRepo.GetIncludeById(departmentId, "SubAdmin");
            if (department == null)
            {
                throw new ArgumentException($"A Department with ID {departmentId} not found.");
            }
            if (department.SubAdmin.Email != subAdminEmail)
            {
                throw new UnauthorizedAccessException("Access Denied: You are not the assigned SubAdmin for this department.");
            }

            var users = _userDepartmentRepo.GetAllAsync().Result.Where(d => d.departmentId == departmentId).Select(u => u.userId).ToList();

            var student = await _studentRepo.SelecteOne(s => s.Email == userName);

            //mybe are teacher or student
            if (student == null || !users.Contains(student.Id) || !await _userManager.IsInRoleAsync(student, Roles.Student.ToString()))
            {
                throw new ArgumentException($"Student with email {userName} not found.");
            }
        
           

            return new StudentViewDetailsDto()
            {
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email,
                DateOfBirth = student.DateOfBirth,
                RegistraionDate = student.RegistraionDate
            };
        }
        public async Task<int> GetStudentsCounts()
        {
            var students = await _userManager.GetUsersInRoleAsync(Roles.Student.ToString());
            return students.Count();
        }
    }
}
