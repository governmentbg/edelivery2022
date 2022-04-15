using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ED.Blobs
{
    public class RolesPerAuthSchemeRequirement
        : IAuthorizationRequirement
    {
        public RolesPerAuthSchemeRequirement(
            IDictionary<string, IEnumerable<string>> allowedRoles)
        {
            this.AllowedRoles =
                allowedRoles 
                ?? throw new ArgumentNullException(nameof(allowedRoles));
        }

        public IDictionary<string, IEnumerable<string>> AllowedRoles { get; }
    }

    public class RolesPerAuthSchemeRequirementHanlder
        : AuthorizationHandler<RolesPerAuthSchemeRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            RolesPerAuthSchemeRequirement requirement)
        {
            string? authScheme =
                context.User?.FindFirst(ClaimTypes.AuthenticationMethod)?.Value;

            if (!string.IsNullOrEmpty(authScheme) &&
                requirement.AllowedRoles.ContainsKey(authScheme) &&
                (!requirement.AllowedRoles[authScheme].Any() ||
                requirement.AllowedRoles[authScheme].Any(r => context.User?.IsInRole(r) == true)))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
