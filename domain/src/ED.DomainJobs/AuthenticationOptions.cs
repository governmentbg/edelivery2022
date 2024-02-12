using System;
using System.ComponentModel.DataAnnotations;

namespace ED.DomainJobs
{
    public class AuthenticationOptions
    {
        [Required]
        public TimeSpan BlobTokenLifetime { get; set; }

        public string? SharedSecretDPKey { get; set; }
    }
}
