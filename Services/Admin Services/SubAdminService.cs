using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using QuizHub.Constant;
using QuizHub.Data.Repository.Base;
using QuizHub.Models;
using QuizHub.Models.DTO.College;
using QuizHub.Models.DTO.User.SubAdmin;
using System.Security.Policy;

namespace QuizHub.Services.Admin_Services.Interface
{
    public class SubAdminService : ISubAdminService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IRepository<AppUser> _repoUser;

        public SubAdminService(UserManager<AppUser> userManager,IRepository<AppUser> repoUser)
        {
            _userManager = userManager;
            _repoUser = repoUser;
        }
        public async Task<GetSubAdminDto> CreateSubAdminAsync(CreateSubAdminDto model)
        {
            var existingSubAdmin = await _userManager.FindByEmailAsync(model.Email);
            if (existingSubAdmin != null)
            {
                return null; 
            }

            if (model.DateOfBirth > DateTime.UtcNow)
            {
                throw new ArgumentException("Date of birth cannot be in the future.");
            }

            var newSubAdmin = new AppUser
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.Email, 
                RegistraionDate = DateTime.UtcNow, 
                DateOfBirth = model.DateOfBirth
            };

            var createResult = await _userManager.CreateAsync(newSubAdmin, model.PassWord);
            if (!createResult.Succeeded)
            {
                throw new Exception($"Failed to create SubAdmin: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }

            var roleResult = await _userManager.AddToRoleAsync(newSubAdmin, Roles.SubAdmin.ToString());
            if (!roleResult.Succeeded)
            {
                throw new Exception($"Failed to assign role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }

            return new GetSubAdminDto() { 
                Email = newSubAdmin.Email,
                FirstName = newSubAdmin.FirstName,
                LastName = newSubAdmin.LastName,
                RegistraionDate = newSubAdmin.RegistraionDate,
                DateOfBirth = newSubAdmin.DateOfBirth,
            };
        }


        public async Task<bool> DeleteSubAdminAsync(string userName)
        {
            AppUser user =await _userManager.FindByEmailAsync(userName);
            if (user == null) {
                return false;
            }
            await _userManager.DeleteAsync(user);
            return true;
        }

        public async Task<GetSubAdminDto> EditSubAdminAsync(string userName, UpdateSubAdminDto model)
        {
            var subAdmin = await _userManager.FindByEmailAsync(userName);
            if (subAdmin == null)
            {
                return null;
            }

            subAdmin.FirstName = model.FirstName ?? subAdmin.FirstName;
            subAdmin.LastName = model.LastName ?? subAdmin.LastName;
            subAdmin.Email = model.Email ?? subAdmin.Email;
            subAdmin.UserName = model.Email ?? subAdmin.UserName;
            subAdmin.DateOfBirth = model.DateOfBirth ?? subAdmin.DateOfBirth;

            if (!string.IsNullOrWhiteSpace(model.PassWord))
            {

                var passwordChangeResult = await _userManager.RemovePasswordAsync(subAdmin);
                if (!passwordChangeResult.Succeeded)
                {
                    return null;
                }

                var addPasswordResult = await _userManager.AddPasswordAsync(subAdmin, model.PassWord);
                if (!addPasswordResult.Succeeded)
                {
                    return null;
                }
            }

            var updateResult = await _userManager.UpdateAsync(subAdmin);
            return updateResult.Succeeded ? new GetSubAdminDto()
            {
                Email = subAdmin.Email,
                FirstName = subAdmin.FirstName,
                LastName = subAdmin.LastName,
                RegistraionDate = subAdmin.RegistraionDate,
                DateOfBirth = subAdmin.DateOfBirth
            } : null;

        }
        public async Task<GetSubAdminDto>GetSubAdminByNameAsync(string userName)
        {
            AppUser userIsFind =await _userManager.FindByEmailAsync(userName);
            if (userIsFind == null)
            {
                return null;
            }
            AppUser user = _repoUser.GetAllIncludeAsync("departments").Result.FirstOrDefault(u => u.Email == userName);
            return new GetSubAdminDto()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth,
                RegistraionDate = user.RegistraionDate,
                DepartmentName = user.departments.First().Name,
            };
        }

        public async Task<IEnumerable<GetSubAdminDto>> GetAllSubAdmin()
        {
            var users = await _repoUser.GetAllIncludeAsync("departments");

            var filteredSubAdmins = users
                .Where(u => _userManager.IsInRoleAsync(u, Roles.SubAdmin.ToString()).Result)
                .Select(x => new GetSubAdminDto
                {
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Email = x.Email,
                    DateOfBirth = x.DateOfBirth,
                    RegistraionDate = x.RegistraionDate,
                    DepartmentName = string.Join(", ", x.departments.Select(d => d.Name))
                });

            return filteredSubAdmins;
        }

    
    }
}
