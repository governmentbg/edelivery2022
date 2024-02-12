using System;
using System.Security.Cryptography.X509Certificates;

namespace ED.Domain
{
    public class DomainOptions
    {
#pragma warning disable CA1024 // Use properties where appropriate
        public string GetConnectionString()
#pragma warning restore CA1024 // Use properties where appropriate
        {
            return this.ConnectionString
                ?? throw new DomainException($"Missing {nameof(DomainOptions)}.{nameof(DomainOptions.ConnectionString)} setting.");
        }

#pragma warning disable CA1721 // Property names should not match get methods
        public string? ConnectionString { get; set; }
#pragma warning restore CA1721 // Property names should not match get methods

        public string? KeystoreServiceUrl { get; set; }

        public bool KeystoreServiceUseGrpcWeb { get; set; }

        public string? BlobsServiceUrl { get; set; }

        public bool BlobsServiceUseGrpcWeb { get; set; }

        public TimeSpan ProfileKeyExpiration { get; set; }

        public string? TimestampServiceUrl { get; set; }

        public int HiLoBlockSize { get; set; }

        public string? WebPortalUrl { get; set; }

        public string? SigningCertificateStore { get; set; }

        public StoreLocation? SigningCertificateStoreLocation { get; set; }

        public string? SigningCertificateThumprint { get; set; }

        public DomainClientsOptions Clients { get; set; } = null!;
    }

    public class DomainClientsOptions
    {
        public EsbOptions Esb { get; set; } = null!;
    }
}
