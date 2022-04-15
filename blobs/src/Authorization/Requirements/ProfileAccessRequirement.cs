using System;
using System.Threading.Tasks;
using ED.DomainServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using static ED.DomainServices.Authorization;

namespace ED.Blobs
{
    public class ProfileAccessRequirement : IAuthorizationRequirement
    {
    }

    public class ProfileAccessRequirementHandler
        : AuthorizationHandler<ProfileAccessRequirement>
    {
        private IServiceProvider serviceProvider;
        public ProfileAccessRequirementHandler(
            IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ProfileAccessRequirement requirement)
        {
            RequirementContext requirementContext =
                AuthorizationHelper.GetRequirementContext(context.Resource);

            int? profileId = (int?)requirementContext.Get(RequirementContextItems.ProfileId);
            int? loginId = (int?)requirementContext.Get(RequirementContextItems.LoginId);

            if (profileId != null &&
                loginId != null &&
                await this.HasProfileAccessAsync(profileId.Value, loginId.Value))
            {
                context.Succeed(requirement);
            }
        }

        private async Task<bool> HasProfileAccessAsync(int profileId, int loginId)
        {
            AuthorizationClient authorizationClient =
                this.serviceProvider.GetRequiredService<AuthorizationClient>();

            var resp = await authorizationClient.HasProfileAccessAsync(
                new HasProfileAccessRequest
                {
                    ProfileId = profileId,
                    LoginId = loginId,
                });

            return resp.HasAccess;
        }
    }
}
