using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;

#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations

namespace ED.Blobs
{
    public class EDeliveryIdentitySecurityToken : SecurityToken
    {
        private AuthenticationTicket authenticationTicket;
        public EDeliveryIdentitySecurityToken(AuthenticationTicket authenticationTicket)
        {
            this.authenticationTicket = authenticationTicket;
        }

        public override string Id =>
            ((ClaimsIdentity?)this.authenticationTicket.Principal.Identity)
            ?.FindFirst("AspNet.Identity.SecurityStamp")
            ?.Value
            ?? throw new NotSupportedException();

        public override string Issuer => "EDeliveryIdentity";

        public override SecurityKey SecurityKey
            => throw new NotImplementedException();

        public override SecurityKey SigningKey
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override DateTime ValidFrom
            => this.authenticationTicket.Properties.IssuedUtc!.Value.LocalDateTime;

        public override DateTime ValidTo
            => this.authenticationTicket.Properties.ExpiresUtc!.Value.LocalDateTime;
    }
}
