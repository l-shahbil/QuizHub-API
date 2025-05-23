using Microsoft.AspNetCore.Identity;
using System.Data;
using System.Security.Claims;

namespace QuizHub.Constant
{
    public static class Permission
    {
        public const string PermissionPolicyPrefix = "Permission";

        public static List<string> generateClaimsList(string module)
        {
            return new List<string>
            {
                $"{PermissionPolicyPrefix}.{module}.View",
                $"{PermissionPolicyPrefix}.{module}.Create",
                $"{PermissionPolicyPrefix}.{module}.Edit",
                $"{PermissionPolicyPrefix}.{module}.Delete"
            };


        }
        public static List<string> GeneratePermission(List<string> modules)
        {
            var allPermission = new List<string>();
            foreach (var module in modules)
            {
                allPermission.AddRange(generateClaimsList(module.ToString()));
            }

            return allPermission;
        }
        public static async Task permessionForStudent(this RoleManager<IdentityRole> roleManager)
        {
            var studentRole = await roleManager.FindByNameAsync(Roles.Student.ToString());
            var allClaims = await roleManager.GetClaimsAsync(studentRole);
            var allPermission = new List<string>();

            allPermission.Add($"{PermissionPolicyPrefix}.{Modules.College}.View");
            allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Department}.View");
            allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Class}.View");
            allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Notification}.View");

            allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Exam}.View Available");
            allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Exam}.Take");
            allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Exam}.Submit");
            allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Exam}.View Previous");
            allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Exam}.Result");
            allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Exam}.Practices");

            allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Notification}.View");
            allPermission.Add($"{PermissionPolicyPrefix}.{Modules.LearingOutcomes}.View");

            foreach (var permission in allPermission)
            {
                if (!allClaims.Any(c => c.Type == PermissionPolicyPrefix && c.Value == permission))
                {
                    await roleManager.AddClaimAsync(studentRole, new Claim(PermissionPolicyPrefix, permission));
                }
            }
        }

        public static async Task addPermissionClaims(this RoleManager<IdentityRole> roleManager, IdentityRole role, List<string> modules)
        {
            var allClaims = await roleManager.GetClaimsAsync(role);

            var allPermission = new List<string>();

            if (modules != null) { 
             allPermission.AddRange(GeneratePermission(modules));
            }


            if (role.Name == Roles.Admin.ToString())
            {
                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.SubAdmin}.Add To Department");
                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.SubAdmin}.Delete From Department");

                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Teacher}.Add To Department");
                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Teacher}.Delete From Department");

                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Subject}.Add To Department");
                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Subject}.Delete From Department");

                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Subject}.Delete From Department");



            }

            else if (role.Name == Roles.SubAdmin.ToString())
            {
                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.College}.View");
                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Department}.View");

                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Teacher}.View");

                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Exam}.Display Report");
                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Exam}.Puplish Exam");
                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Exam}.View Available");
                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Exam}.Result");


                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Subject}.View");
                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.LearingOutcomes}.View");

                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Student}.Add To Batch");
                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Student}.Delete From Batch");
                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Student}.Get All Student In Batch");

                

            }
            else if (role.Name == Roles.Teacher.ToString())
            {
                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.College}.View");
                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Department}.View");

                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Exam}.Display Attendance");
                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Exam}.Shedule Exam");
                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Exam}.View Available");
                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Exam}.Result");
                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Exam}.Puplish Exam");
                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Exam}.View");

                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Subject}.View");

                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Class}.View");
                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.LearingOutcomes}.View");

                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Student}.View");


            }
            else if (role.Name == Roles.Student.ToString())
            {
                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.College}.View");
                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Department}.View");
                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Class}.View");
                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Notification}.View");

                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Exam}.View Available");
                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Exam}.Take");
                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Exam}.View Previous");
                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Exam}.Result");
                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Exam}.Practices");

                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.Notification}.View");
                allPermission.Add($"{PermissionPolicyPrefix}.{Modules.LearingOutcomes}.View");


            }

            foreach (var permission in allPermission)
            {
                if (!allClaims.Any(c => c.Type == PermissionPolicyPrefix && c.Value == permission))
                {
                    await roleManager.AddClaimAsync(role, new Claim(PermissionPolicyPrefix, permission));
                }
            }


        }

    }
}
