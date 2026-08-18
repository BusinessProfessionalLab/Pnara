using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace WebApi.Authorization;

public class PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    public const string PolicyPrefix = "perm:";

    public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith(PolicyPrefix, StringComparison.OrdinalIgnoreCase))
            return base.GetPolicyAsync(policyName);

        var permission = policyName[PolicyPrefix.Length..];

        return Task.FromResult<AuthorizationPolicy?>(
            new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "permission" && c.Value == permission) ||
                    context.User.HasClaim(c => c.Type == "role" && c.Value == SystemRoles.Admin))
                .Build());
    }
}
