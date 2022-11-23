using System;
using System.Security.Claims;

namespace ED.Blobs
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetAuthenticatedUserProfileId(this ClaimsPrincipal user)
        {
            return user.GetAuthenticatedUserProfileIdOrDefault()
                ?? throw new Exception("User is not authenticated.");
        }

        public static int? GetAuthenticatedUserProfileIdOrDefault(this ClaimsPrincipal user)
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

        public static int? GetAuthenticatedUserRepresentedProfileId(
            this ClaimsPrincipal user)
        {
            if (user.Identity?.IsAuthenticated != true)
            {
                throw new Exception($"Missing {EsbAuthClaimTypes.RepresentedProfileId} claim");
            }

            Claim claim =
                user.FindFirst(EsbAuthClaimTypes.RepresentedProfileId)
                    ?? throw new Exception($"Missing {EsbAuthClaimTypes.RepresentedProfileId} claim");

            return int.TryParse(claim.Value, out int representedProfileId)
                ? (int?)representedProfileId
                : null;
        }

        public static int GetAuthenticatedUserLoginId(this ClaimsPrincipal user)
        {
            if (user.Identity?.IsAuthenticated != true)
            {
                throw new Exception($"Missing {EsbAuthClaimTypes.LoginId} claim");
            }

            Claim claim =
                user.FindFirst(EsbAuthClaimTypes.LoginId)
                    ?? throw new Exception($"Missing {EsbAuthClaimTypes.LoginId} claim");

            return int.Parse(claim.Value);
        }

        public static int? GetAuthenticatedUserOperatorLoginId(this ClaimsPrincipal user)
        {
            if (user.Identity?.IsAuthenticated != true)
            {
                throw new Exception($"Missing {EsbAuthClaimTypes.OperatorLoginId} claim");
            }

            Claim claim =
                user.FindFirst(EsbAuthClaimTypes.OperatorLoginId)
                    ?? throw new Exception($"Missing {EsbAuthClaimTypes.OperatorLoginId} claim");

            return int.Parse(claim.Value);
        }
    }
}
