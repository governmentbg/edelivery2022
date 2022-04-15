using System;
using System.ComponentModel.DataAnnotations;

#nullable enable

namespace ED.AdminPanel
{
    public class AdminPanelOptions
    {
        [Required]
        public string DomainServicesUrl { get; set; } = null!;

        public bool DomainServicesUseGrpcWeb { get; set; }

        [Required]
        public string BlobServiceWebUrl { get; set; } = null!;

        public TimeSpan BlobTokenLifetime { get; set; }

        public string? SharedSecretDPKey { get; set; }
    }
}
