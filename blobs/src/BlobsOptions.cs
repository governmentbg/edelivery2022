using System;
using System.Security.Cryptography.X509Certificates;

namespace ED.Blobs
{
    public class BlobsOptions
    {
        public string[]? GrpcServiceHosts { get; set; }

        public bool EnableGrpcWeb { get; set; }
        
        public string? HashAlgorithm { get; set; }

        public string[]? AllowedCorsOrigins { get; set; }

        public string? MalwareServiceCertificateStore { get; set; }

        public StoreLocation? MalwareServiceCertificateStoreLocation { get; set; }

        public string? MalwareServiceCertificateThumprint { get; set; }

        public bool AllowUntrustedCertificates { get; set; }

        public string? MalwareApiUrl { get; set; }

        public bool MalwareScanEnabled { get; set; }

        public int MalwareApiMaxAllowedFileSizeInMb { get; set; }

        public string? DomainServicesUrl { get; set; }

        public bool DomainServicesUseGrpcWeb { get; set; }

        public string? KeystoreServicesUrl { get; set; }

        public bool KeystoreServicesUseGrpcWeb { get; set; }

        public string? TimestampServiceUrl { get; set; }

        public string? SharedSecretDPKey { get; set; }

        public int ExtractSignaturesPdfMaxSizeInMb { get; set; }

        public TimeSpan ProfileKeyExpiration { get; set; }

        public string? PdfServicesUrl { get; set; }
    }
}
