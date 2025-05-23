using Microsoft.AspNetCore.Identity;
using QuizHub.Constant;
using System.Reflection;

namespace QuizHub.Data.Seeds
{
    public static class DefaultClaims
    {
        public static async Task seedsClaims(this RoleManager<IdentityRole> roleManager)
        {
           await  roleManager.seedClaimsToAdmin();
            await roleManager.seedClaimsToSubAdmin();
            await roleManager.seedClaimsToTeacher();
            await roleManager.seedClaimsToStudent();
        }
        public static async Task seedClaimsToAdmin(this RoleManager<IdentityRole> roleManager)
        {
            var modules = new List<string> { Modules.College.ToString(), Modules.Department.ToString(), Modules.Teacher.ToString(), Modules.SubAdmin.ToString(), Modules.Subject.ToString(),Modules.LearingOutcomes.ToString() };

            var role = await roleManager.FindByNameAsync(Roles.Admin.ToString());
            await roleManager.addPermissionClaims(role, modules);
        }

        public static async Task seedClaimsToSubAdmin(this RoleManager<IdentityRole> roleManager)
        {
            var modules = new List<string> { Modules.Student.ToString(), Modules.Question.ToString(), Modules.Class.ToString(), Modules.Batch.ToString(), Modules.Notification.ToString(), Modules.Exam.ToString() };

            var role = await roleManager.FindByNameAsync(Roles.SubAdmin.ToString());
            await roleManager.addPermissionClaims(role, modules);
        }
        public static async Task seedClaimsToTeacher(this RoleManager<IdentityRole> roleManager)
        {
            var modules = new List<string> { Modules.Question.ToString(), Modules.Exam.ToString(), Modules.Notification.ToString() };

            var role = await roleManager.FindByNameAsync(Roles.Teacher.ToString());
            await roleManager.addPermissionClaims(role, modules);
        }

        public static async Task seedClaimsToStudent(this RoleManager<IdentityRole> roleManager)
        {

            var role = await roleManager.FindByNameAsync(Roles.Student.ToString());
            await roleManager.permessionForStudent();
        }
    }
}
