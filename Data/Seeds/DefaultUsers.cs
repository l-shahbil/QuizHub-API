using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuizHub.Constant;
using QuizHub.Models;
using System.Reflection;
using System.Security.AccessControl;

namespace QuizHub.Data.Seeds
{
    public static class DefaultUsers
    {
        public static async Task seedsAdminAsync(UserManager<AppUser> userManger,RoleManager<IdentityRole> roleManger)
        {
            var admin = new AppUser
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = "Admin",
                LastName = "QuizHub",
                RegistraionDate = DateTime.Now,
                DateOfBirth = DateTime.Now,
                UserName = "quizHubAdmin",
                Email = "quizHubAdmin@quiz.com",
                EmailConfirmed = true
            };

            var result = await userManger.FindByEmailAsync(admin.Email);
            if (result == null)
            {
                await userManger.CreateAsync(admin,"@quizHub777173");
                await userManger.AddToRoleAsync(admin,Roles.Admin.ToString());
            }

           
        }
       
    }
}
