using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace ED.EsbApi;

public class BlobUrlCreator
{
    private const string MessageBlobTokenPurpose = "ED.Blobs.MessageBlobToken";

    private readonly ITimeLimitedDataProtector systemBlobTokenDataProtector;
    private readonly string blobServiceWebUrl;
    private readonly TimeSpan blobTokenLifetime;

    public BlobUrlCreator(
        IDataProtector dataProtector,
        IOptions<EsbApiOptions> optionsAccessor)
    {
        this.systemBlobTokenDataProtector =
            dataProtector.CreateProtector(MessageBlobTokenPurpose)
                .ToTimeLimitedDataProtector();

        EsbApiOptions options = optionsAccessor.Value;
        this.blobServiceWebUrl = options.BlobServiceWebUrl;
        this.blobTokenLifetime = options.BlobTokenLifetime;
    }

    public (string, DateTime) CreateMessageBlobUrl(
        int profileId,
        int messageId,
        int blobId)
    {
        byte[] protectedData = CompactPositionalSerializer.Serialize(
            profileId,
            messageId,
            blobId);

        byte[] tokenBytes =
            this.systemBlobTokenDataProtector.Protect(
                protectedData,
                this.blobTokenLifetime);

        DateTime expirationDate = DateTime.Now.Add(this.blobTokenLifetime);

        string token = ToUrlSafeBase64(tokenBytes);

        return (
            $"{this.blobServiceWebUrl.TrimEnd('/')}/message?t={token}",
            expirationDate
        );
    }

    private static string ToUrlSafeBase64(byte[] data)
        => Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
