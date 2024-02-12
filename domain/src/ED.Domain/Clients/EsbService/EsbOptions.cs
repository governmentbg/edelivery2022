using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

namespace ED.Domain
{
    public class EsbOptions
    {
        [Required]
        public string ApiUrl { get; set; } = null!;

        [Required]
        public string TokenApiUrl { get; set; } = null!;

        [Required]
        public string ServiceCertificateStore { get; set; } = null!;

        [Required]
        public StoreLocation? ServiceCertificateStoreLocation { get; set; } = null!;

        [Required]
        public string ServiceCertificateThumbprint { get; set; } = null!;

        [Required]
        public bool AllowUntrustedCertificates { get; set; }

        [Required]
        public string ClientId { get; set; } = null!;

        [Required]
        public string Oid { get; set; } = null!;

        [Required]
        public string OidName { get; set; } = null!;
    }
}
