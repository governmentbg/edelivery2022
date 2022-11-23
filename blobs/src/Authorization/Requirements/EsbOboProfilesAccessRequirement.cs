using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using static ED.DomainServices.Esb.Esb;

namespace ED.Blobs
{
    public class EsbOboProfilesAccessRequirement : IAuthorizationRequirement
    {
    }

    public class EsbOboProfilesAccessRequirementHandler
        : AuthorizationHandler<EsbOboProfilesAccessRequirement>
    {
        private IServiceProvider serviceProvider;
        public EsbOboProfilesAccessRequirementHandler(
            IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            EsbOboProfilesAccessRequirement requirement)
        {
            int profileId = context.User.GetAuthenticatedUserProfileId();

            EsbClient esbClient =
                this.serviceProvider.GetRequiredService<EsbClient>();

            DomainServices.Esb.CheckProfileOnBehalfOfAccessResponse resp =
                await esbClient.CheckProfileOnBehalfOfAccessAsync(
                    new DomainServices.Esb.CheckProfileOnBehalfOfAccessRequest
                    {
                        ProfileId = profileId,
                    });

            if (resp.HasAccess)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return;
        }
    }
}
