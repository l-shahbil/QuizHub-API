using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using QuizHub.Constant;

namespace QuizHub.Services.Authorization.Policies
{
    public class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; set; }

        public PermissionPolicyProvider(IOptions<AuthorizationOptions> option)
        {
            FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(option);
        }
        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return FallbackPolicyProvider.GetDefaultPolicyAsync();
        }

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        {
            return FallbackPolicyProvider.GetDefaultPolicyAsync();
        }

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)

        {
            if (policyName.StartsWith(Permission.PermissionPolicyPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var policy = new AuthorizationPolicyBuilder();
                policy.AddRequirements(new PermissionRequirement(policyName));
                return Task.FromResult(policy.Build());
            }
            return FallbackPolicyProvider.GetPolicyAsync(policyName);
        }
    }
}
