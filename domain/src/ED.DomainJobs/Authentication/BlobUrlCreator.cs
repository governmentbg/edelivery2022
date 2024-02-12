using System;
using System.Threading;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace ED.DomainJobs
{
    public class BlobUrlCreator
    {
        private const string ProfileBlobTokenPurpose = "ED.Blobs.ProfileBlobToken";

        private readonly Lazy<ITimeLimitedDataProtector> profileBlobTokenDataProtector;

        private readonly TimeSpan blobTokenLifetime;

        public BlobUrlCreator(
            IDataProtector dataProtector,
            IOptions<AuthenticationOptions> optionsAccessor)
        {
            this.profileBlobTokenDataProtector = new Lazy<ITimeLimitedDataProtector>(
                () => dataProtector.CreateProtector(ProfileBlobTokenPurpose).ToTimeLimitedDataProtector(),
                LazyThreadSafetyMode.None);

            this.blobTokenLifetime = optionsAccessor.Value.BlobTokenLifetime;
        }

        public string CreateBlobToken(
            int messageTranslationId)
        {
            return this.CreateToken(
                this.profileBlobTokenDataProtector.Value,
                messageTranslationId);
        }

        private string CreateToken(
            ITimeLimitedDataProtector dataProtector,
            params object[] tokenValues)
        {
            byte[] protectedData = CompactPositionalSerializer.Serialize(tokenValues);

            byte[] tokenBytes = dataProtector.Protect(
                protectedData,
                this.blobTokenLifetime);

            string token = ToUrlSafeBase64(tokenBytes);

            return token;
        }

        private static string ToUrlSafeBase64(byte[] data)
            => Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
