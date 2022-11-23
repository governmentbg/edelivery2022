using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ED.Blobs
{
    public class EsbStrictDenyAnonymousAuthorizationRequirement : IAuthorizationRequirement
    {
    }

    public class EsbStrictDenyAnonymousAuthorizationRequirementHandler : AuthorizationHandler<EsbStrictDenyAnonymousAuthorizationRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            EsbStrictDenyAnonymousAuthorizationRequirement requirement)
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
}
