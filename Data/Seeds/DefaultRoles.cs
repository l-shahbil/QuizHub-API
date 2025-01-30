using Microsoft.AspNetCore.Identity;
using QuizHub.Constant;

namespace QuizHub.Data.Seeds
{
    public static class DefaultRoles    
    {
        public static async Task SeedRoleAsync(RoleManager<IdentityRole> roleManger)
        {
            if (!roleManger.Roles.Any())
            {
                await roleManger.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
                await roleManger.CreateAsync(new IdentityRole(Roles.SubAdmin.ToString()));
                await roleManger.CreateAsync(new IdentityRole(Roles.Teacher.ToString()));
                await roleManger.CreateAsync(new IdentityRole(Roles.Student.ToString()));
            }
        }
    }
}
