using System;
using System.Security.Claims;

namespace ED.EsbApi;

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

    public static int GetAuthenticatedUserRepresentedProfileId(
        this ClaimsPrincipal user)
    {
        return user.GetAuthenticatedUserRepresentedProfileIdOrDefault()
            ?? throw new Exception("User has no representing profile.");
    }

    public static int? GetAuthenticatedUserRepresentedProfileIdOrDefault(
        this ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return null;
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
        return user.GetAuthenticatedUserLoginIdOrDefault()
            ?? throw new Exception("User has no login.");
    }

    public static int? GetAuthenticatedUserLoginIdOrDefault(this ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        Claim claim =
            user.FindFirst(EsbAuthClaimTypes.LoginId)
                ?? throw new Exception($"Missing {EsbAuthClaimTypes.LoginId} claim");

        return int.Parse(claim.Value);
    }

    public static int GetAuthenticatedUserOperatorLoginId(this ClaimsPrincipal user)
    {
       return user.GetAuthenticatedUserOperatorLoginIdOrDefault()
            ?? throw new Exception("User has no operator.");
    }

    public static int? GetAuthenticatedUserOperatorLoginIdOrDefault(this ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        Claim claim =
            user.FindFirst(EsbAuthClaimTypes.OperatorLoginId)
                ?? throw new Exception($"Missing {EsbAuthClaimTypes.OperatorLoginId} claim");

        return int.TryParse(claim.Value, out int operatorLoginId)
            ? operatorLoginId
            : null;
    }
}
