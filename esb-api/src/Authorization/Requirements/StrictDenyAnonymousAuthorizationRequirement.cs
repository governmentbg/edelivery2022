using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ED.EsbApi;

public class StrictDenyAnonymousAuthorizationRequirement : IAuthorizationRequirement
{
}

public class StrictDenyAnonymousAuthorizationRequirementHander : AuthorizationHandler<StrictDenyAnonymousAuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        StrictDenyAnonymousAuthorizationRequirement requirement)
    {
        var user = context.User;
        var userIsAnonymous =
            user?.Identity == null ||
            !user.Identities.Any(i => i.IsAuthenticated);
        if (!userIsAnonymous)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}
