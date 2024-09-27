using System;
using System.Threading;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace ED.EsbApi;

public class BlobUrlCreator
{
    private const string MessageBlobTokenPurpose = "ED.Blobs.MessageBlobToken";
    private const string ProfileBlobTokenPurpose = "ED.Blobs.ProfileBlobToken";

    private readonly Lazy<ITimeLimitedDataProtector> messageBlobTokenDataProtector;
    private readonly Lazy<ITimeLimitedDataProtector> profileBlobTokenDataProtector;

    private readonly string blobServiceWebUrl;
    private readonly TimeSpan blobTokenLifetime;

    public BlobUrlCreator(
        IDataProtector dataProtector,
        IOptions<EsbApiOptions> optionsAccessor)
    {
        this.messageBlobTokenDataProtector = new Lazy<ITimeLimitedDataProtector>(
            () => dataProtector.CreateProtector(MessageBlobTokenPurpose).ToTimeLimitedDataProtector(),
            LazyThreadSafetyMode.ExecutionAndPublication);

        this.profileBlobTokenDataProtector = new Lazy<ITimeLimitedDataProtector>(
            () => dataProtector.CreateProtector(ProfileBlobTokenPurpose).ToTimeLimitedDataProtector(),
            LazyThreadSafetyMode.ExecutionAndPublication);

        EsbApiOptions options = optionsAccessor.Value;
        this.blobServiceWebUrl = options.BlobServiceWebUrl;
        this.blobTokenLifetime = options.BlobTokenLifetime;
    }

    public (string, DateTime) CreateMessageBlobUrl(
        int profileId,
        int messageId,
        int blobId)
    {
        return this.CreateBlobUrl(
            this.messageBlobTokenDataProtector.Value,
            "message",
            profileId,
            messageId,
            blobId);
    }

    public (string, DateTime) CreateProfileBlobUrl(
        int profileId,
        int blobId)
    {
        return this.CreateBlobUrl(
            this.profileBlobTokenDataProtector.Value,
            "profile",
            profileId,
            blobId);
    }

    private (string, DateTime) CreateBlobUrl(
        ITimeLimitedDataProtector dataProtector,
        string path,
        params object[] tokenValues)
    {
        byte[] protectedData = CompactPositionalSerializer.Serialize(tokenValues);

        byte[] tokenBytes = dataProtector.Protect(
            protectedData,
            this.blobTokenLifetime);

        DateTime expirationDate = DateTime.Now.Add(this.blobTokenLifetime);

        string token = ToUrlSafeBase64(tokenBytes);

        return (
            $"{this.blobServiceWebUrl.TrimEnd('/')}/{path.Trim('/')}?t={token}",
            expirationDate
        );
    }

    private static string ToUrlSafeBase64(byte[] data)
        => Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
