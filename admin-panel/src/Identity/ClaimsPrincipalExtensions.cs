using System;
using System.Security.Claims;

#nullable enable

namespace ED.AdminPanel
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetAuthenticatedUserId(this ClaimsPrincipal user)
        {
            return user.GetAuthenticatedUserIdOrDefault()
                ?? throw new Exception("User is not authenticated.");
        }

        public static int? GetAuthenticatedUserIdOrDefault(this ClaimsPrincipal user)
        {
            if (user.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            Claim nameIdentifierClaim =
                user.FindFirst(ClaimTypes.NameIdentifier)
                ?? throw new Exception("Missing NameIdentifier claim");

            return int.Parse(nameIdentifierClaim.Value);
        }
    }
}
