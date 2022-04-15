using System;
using System.Configuration;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Runtime.Caching;
using System.Security.Cryptography.X509Certificates;
using ED.DomainServices.IntegrationService;
using log4net;

namespace ED.IntegrationService
{
    /// <summary>
    /// Validates the client certificate of the request
    /// </summary>
    public class ESubjectCertificateValidator : X509CertificateValidator
    {
        private static readonly ILog logger = LogManager.GetLogger(nameof(ESubjectCertificateValidator));

        private bool ValidateCertificate { get; set; }

        private TimeSpan CacheExpiration { get; set; }

        public ESubjectCertificateValidator()
        {
            if (bool.TryParse(
                ConfigurationManager.AppSettings["ValidateCertificate"],
                out bool validateCertificate))
            {
                this.ValidateCertificate = validateCertificate;
            }

            if (TimeSpan.TryParse(
                ConfigurationManager.AppSettings["CertificateValidatorCacheExpiration"],
                out TimeSpan cacheSlidingExpiration))
            {
                this.CacheExpiration = cacheSlidingExpiration;
            }
        }

        public override void Validate(X509Certificate2 certificate)
        {
            bool isAccepted = MemoryCacheExtensions.GeneralPurposeCache.AddOrGetExisting(
                certificate.Thumbprint,
                () => IsAccepted(certificate, this.ValidateCertificate),
                new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.Now.Add(this.CacheExpiration)
                });

            if (!isAccepted)
            {
                throw new SecurityTokenValidationException();
            }

            return;
        }

        private static bool IsAccepted(
            X509Certificate2 certificate,
            bool validateCertificate)
        {
            if (validateCertificate)
            {
                var valid = new X509Chain().Build(certificate);
                if (!valid)
                {
                    return false;
                }
            }

            try
            {
                var client = GrpcClientFactory.CreateIntegrationServiceClient();
                HasLoginWithCertificateThumbprintResponse resp =
                    client.HasLoginWithCertificateThumbprint(
                        new HasLoginWithCertificateThumbprintRequest
                        {
                            CertificateThumbprint = certificate.Thumbprint,
                        });

                return resp.HasLogin;
            }
            catch (Exception ex)
            {
                logger.Error("Grpc call to HasLoginWithCertificateThumbprint failed.", ex);

                // throwing an exeption will not be cached
                // and the method will be executed again
                throw;
            }
            
        }
    }
}
