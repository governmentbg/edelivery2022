using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace ED.AdminPanel
{
    public class BlobUrlCreator
    {
        private const string SystemBlobTokenPurpose = "ED.Blobs.SystemBlobToken";

        private readonly ITimeLimitedDataProtector systemBlobTokenDataProtector;
        private readonly string blobServiceWebUrl;
        private readonly TimeSpan blobTokenLifetime;

        public BlobUrlCreator(
            IDataProtector dataProtector,
            IOptions<AdminPanelOptions> optionsAccessor)
        {
            this.systemBlobTokenDataProtector =
                dataProtector.CreateProtector(SystemBlobTokenPurpose)
                    .ToTimeLimitedDataProtector();

            AdminPanelOptions options = optionsAccessor.Value;
            this.blobServiceWebUrl = options.BlobServiceWebUrl;
            this.blobTokenLifetime = options.BlobTokenLifetime;
        }

        public string CreateSystemBlobUrl(int blobId)
        {
            byte[] protectedData = CompactPositionalSerializer.Serialize(blobId);

            byte[] tokenBytes =
                this.systemBlobTokenDataProtector.Protect(
                    protectedData,
                    this.blobTokenLifetime);

            string token = ToUrlSafeBase64(tokenBytes);

            return $"{this.blobServiceWebUrl.TrimEnd('/')}/system?t={token}";
        }

        private static string ToUrlSafeBase64(byte[] data)
            => Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
