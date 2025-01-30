using Microsoft.AspNetCore.Authorization;
using QuizHub.Constant;

namespace QuizHub.Services.Authorization.Policies
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IConfiguration _config;

        public PermissionAuthorizationHandler(IConfiguration config)
        {
            _config = config;
        }
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.User == null)
                return;
         
            var canAccess = context.User.Claims.Any(c => c.Type == Permission.PermissionPolicyPrefix && c.Value == requirement.Permission && c.Issuer == _config["JWT:validIssure"]);
            if (canAccess)
            {
                context.Succeed(requirement);
                return;
            }
        }
    }
}
