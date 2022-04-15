using System;
using System.Threading.Tasks;
using ED.DomainServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using static ED.DomainServices.Authorization;

namespace ED.Blobs
{
    public class DenyReadonlyProfileRequirement : IAuthorizationRequirement
    {
    }

    public class DenyReadonlyProfileRequirementHandler
        : AuthorizationHandler<DenyReadonlyProfileRequirement>
    {
        private IServiceProvider serviceProvider;
        public DenyReadonlyProfileRequirementHandler(
            IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            DenyReadonlyProfileRequirement requirement)
        {
            RequirementContext requirementContext =
                AuthorizationHelper.GetRequirementContext(context.Resource);

            int? profileId = (int?)requirementContext.Get(RequirementContextItems.ProfileId);

            if (profileId != null &&
                !await this.IsReadonlyProfileAsync(profileId.Value))
            {
                context.Succeed(requirement);
            }
        }

        private async Task<bool> IsReadonlyProfileAsync(int profileId)
        {
            AuthorizationClient authorizationClient =
                this.serviceProvider.GetRequiredService<AuthorizationClient>();

            var resp = await authorizationClient.IsReadonlyProfileAsync(
                new IsReadonlyProfileRequest
                {
                    ProfileId = profileId,
                });

            return resp.IsReadonly;
        }
    }
}
