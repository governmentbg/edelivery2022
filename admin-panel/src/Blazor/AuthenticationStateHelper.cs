using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;

#nullable enable

namespace ED.AdminPanel.Blazor
{
    public sealed class AuthenticationStateHelper
    {
        private readonly AuthenticationStateProvider authenticationStateProvider;
        public AuthenticationStateHelper(AuthenticationStateProvider authenticationStateProvider)
        {
            this.authenticationStateProvider = authenticationStateProvider;
        }

        public async Task<int> GetAuthenticatedUserId()
        {
            AuthenticationState authenticationState =
                await this.authenticationStateProvider.GetAuthenticationStateAsync();

            return authenticationState.User.GetAuthenticatedUserId();
        }

        public async Task<int?> GetAuthenticatedUserIdOrDefault()
        {
            AuthenticationState authenticationState =
                await this.authenticationStateProvider.GetAuthenticationStateAsync();

            return authenticationState.User.GetAuthenticatedUserIdOrDefault();
        }

        public async Task<ClaimsPrincipal?> GetAuthenticatedUserClaimPrincipal()
        {
            AuthenticationState authenticationState =
                await this.authenticationStateProvider.GetAuthenticationStateAsync();

            if (authenticationState.User.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            return authenticationState.User;
        }
    }
}
