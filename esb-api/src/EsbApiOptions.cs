using System;
using System.ComponentModel.DataAnnotations;

namespace ED.EsbApi;

public class EsbApiOptions
{
    public string DomainServicesUrl { get; set; } = null!;

    public bool DomainServicesUseGrpcWeb { get; set; }

    [Required]
    public string BlobServiceWebUrl { get; set; } = null!;

    public string BlobsServiceUrl { get; set; } = null!;

    public bool BlobsServiceUseGrpcWeb { get; set; }

    public TimeSpan BlobTokenLifetime { get; set; }

    public string? SharedSecretDPKey { get; set; }
}
