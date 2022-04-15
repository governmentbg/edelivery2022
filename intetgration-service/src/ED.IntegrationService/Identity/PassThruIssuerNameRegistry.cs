using System.IdentityModel.Tokens;

namespace ED.IntegrationService
{
    public class PassThruIssuerNameRegistry : IssuerNameRegistry
    {
        public override string GetIssuerName(SecurityToken securityToken)
        {
            if (securityToken is X509SecurityToken tokenCert)
            {
                return tokenCert.Certificate.Issuer;
            }

            return null;
        }
    }
}
