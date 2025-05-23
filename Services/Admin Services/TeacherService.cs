using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using QuizHub.Constant;
using QuizHub.Data.Repository.Base;
using QuizHub.Models;
using QuizHub.Models.DTO.College;
using QuizHub.Models.DTO.User.Teacher;
using System.ComponentModel.DataAnnotations;
using System.Security.Policy;
using System.Text.RegularExpressions;

namespace QuizHub.Services.Admin_Services.Interface
{
    public class TeacherService : ITeacherService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IRepository<AppUser> _repoUser;

        public TeacherService(UserManager<AppUser> userManager,IRepository<AppUser> repoUser)
        {
            _userManager = userManager;
            _repoUser = repoUser;
        }


        public async Task<IEnumerable<GetTeacherDto>> GetAllTeacher()
        {
            var users = await _repoUser.GetAllIncludeAsync("departments");

            var filteredteachers = users
                .Where(u => _userManager.IsInRoleAsync(u, Roles.Teacher.ToString()).Result)
                .Select(x => new GetTeacherDto
                {
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Email = x.Email,
                    DateOfBirth = x.DateOfBirth,
                    RegistraionDate = x.RegistraionDate,
                    DepartmentName = string.Join(", ", x.departments.Select(d => d.Name))
                });

            return filteredteachers;
        }

      

        public async Task<GetTeacherDto> GetTeacherByNameAsync(string userName)
        {
            AppUser userIsFind = await _userManager.FindByEmailAsync(userName);
            if (userIsFind == null)
            {
                return null;
            }
            AppUser user = _repoUser.GetAllIncludeAsync("departments").Result.FirstOrDefault(u => u.Email == userName);
            return new GetTeacherDto()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth,
                RegistraionDate = user.RegistraionDate,
                DepartmentName = string.Join(", ", user.departments.Select(d => d.Name))
            };
        }

        public async Task<GetTeacherDto> CreateTeacherAsync(CreateTeacherDto model)
        {
            var existingTeacher = await _userManager.FindByEmailAsync(model.Email);
            if (existingTeacher != null)
            {
                throw new InvalidOperationException("Teacher with this email already exists.");
            }

            if (model.DateOfBirth > DateTime.UtcNow)
            {
                throw new InvalidOperationException("Date of birth cannot be in the future.");
            }

            var newTeacher = new AppUser
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.Email,
                RegistraionDate = DateTime.UtcNow,
                DateOfBirth = model.DateOfBirth.Value
            };

            var createResult = await _userManager.CreateAsync(newTeacher, model.PassWord);
            if (!createResult.Succeeded)
            {
                throw new Exception($"Failed to create teacher: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }

            var roleResult = await _userManager.AddToRoleAsync(newTeacher, Roles.Teacher.ToString());
            if (!roleResult.Succeeded)
            {
                throw new Exception($"Failed to assign role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }

            return new GetTeacherDto()
            {
                Email = newTeacher.Email,
                FirstName = newTeacher.FirstName,
                LastName = newTeacher.LastName,
                RegistraionDate = newTeacher.RegistraionDate,
                DateOfBirth = newTeacher.DateOfBirth,
            };
        }

        public async Task<GetTeacherDto> EditTeacherAsync(string userName, UpdateTeacherDto model)
        {
            var teacher = await _userManager.FindByEmailAsync(userName);
            if (teacher == null)
            {
                throw new ArgumentException ("Teacher is not found");
            }

            if (!string.IsNullOrEmpty(model.Email))
            {
                string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                if (!Regex.IsMatch(model.Email, emailPattern, RegexOptions.IgnoreCase))
                {
                    throw new ArgumentException("Invalid email address.");
                }


              var newTeacher = await _userManager.FindByEmailAsync(model.Email);
                if (newTeacher != null)
                {
                    throw new ArgumentException("Username already exists.");

                }

                teacher.Email = model.Email;
            }
            
            teacher.FirstName =string.IsNullOrWhiteSpace(model.FirstName)? teacher.FirstName:model.FirstName;
            teacher.LastName = string.IsNullOrWhiteSpace(model.LastName) ? teacher.LastName : model.LastName;
            if (model.DateOfBirth.HasValue && model.DateOfBirth > DateTime.UtcNow) 
            {
                teacher.DateOfBirth = model.DateOfBirth.Value;
            }



            if (!string.IsNullOrWhiteSpace(model.PassWord))
            {
                if (model.PassWord.Length < 7)
                {
                    throw new ArgumentException("Password does not meet the requirements.");

                }
                var passwordChangeResult = await _userManager.RemovePasswordAsync(teacher);
                if (!passwordChangeResult.Succeeded)
                {
                    throw new ArgumentException("Password change failed.");

                }

                var addPasswordResult = await _userManager.AddPasswordAsync(teacher, model.PassWord);
                if (!addPasswordResult.Succeeded)
                {
                    throw new ArgumentException("Password change failed.");
                
                }
            }

            var updateResult = await _userManager.UpdateAsync(teacher);
            return updateResult.Succeeded ? new GetTeacherDto()
            {
                Email = teacher.Email,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                RegistraionDate = teacher.RegistraionDate,
                DateOfBirth = teacher.DateOfBirth
            } : null;

        }

        public async Task<bool> DeleteTeacherAsync(string userName)
        {
            AppUser user = await _userManager.FindByEmailAsync(userName);
            if (user == null)
            {
                return false;
            }
            await _userManager.DeleteAsync(user);
            return true;
        }
    }
}

