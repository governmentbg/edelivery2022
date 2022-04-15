using System;
using System.Security.Claims;
using System.Xml;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;

namespace ED.Blobs
{
    public class LegacyOAuthSecurityTokenHandler : SecurityTokenHandler
    {
        private readonly TicketDataFormat ticketDataFormat;
        private readonly Claim[] additionalStaticClaims;

        public LegacyOAuthSecurityTokenHandler(
            IDataProtector dataProtector,
            params Claim[] additionalStaticClaims)
        {
            this.ticketDataFormat = new TicketDataFormat(dataProtector);
            this.additionalStaticClaims = additionalStaticClaims;
        }

        public override bool CanValidateToken => true;

        public override bool CanReadToken(string tokenString) => true;

        public override ClaimsPrincipal ValidateToken(
            string securityToken,
            TokenValidationParameters validationParameters,
            out SecurityToken validatedToken)
        {
            var authenticationTicket = this.ticketDataFormat.Unprotect(securityToken);

            if (authenticationTicket == null)
            {
                throw new Exception("Could not unprotect token.");
            }

            if (authenticationTicket.Properties.IssuedUtc > DateTimeOffset.UtcNow)
            {
                throw new Exception("Ticket is not valid yet, the IssuedUtc date is in the future.");
            }

            if (authenticationTicket.Properties.ExpiresUtc < DateTimeOffset.UtcNow)
            {
                throw new Exception("Ticket has expired.");
            }

            foreach (var claim in this.additionalStaticClaims)
            {
                ((ClaimsIdentity)authenticationTicket.Principal.Identity!).AddClaim(claim);
            }

            validatedToken = new EDeliveryIdentitySecurityToken(authenticationTicket);

            return authenticationTicket.Principal;
        }

        public override SecurityToken ReadToken(
            XmlReader reader,
            TokenValidationParameters validationParameters)
        {
            throw new NotImplementedException();
        }

        public override void WriteToken(XmlWriter writer, SecurityToken token)
        {
            throw new NotImplementedException();
        }

        public override Type TokenType => typeof(EDeliveryIdentitySecurityToken);
    }
}
