using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

namespace ED.Blobs
{
    public class BlobTokenParser
    {
        private const string MessageBlobTokenPurpose = "ED.Blobs.MessageBlobToken";
        private const string ProfileBlobTokenPurpose = "ED.Blobs.ProfileBlobToken";
        private const string SystemBlobTokenPurpose = "ED.Blobs.SystemBlobToken";

        private readonly ITimeLimitedDataProtector messageBlobTokenDataProtector;
        private readonly ITimeLimitedDataProtector profileBlobTokenDataProtector;
        private readonly ITimeLimitedDataProtector systemBlobTokenDataProtector;
        private readonly ILogger<BlobTokenParser> logger;

        public BlobTokenParser(
            ILogger<BlobTokenParser> logger,
            IDataProtector dataProtector)
        {
            this.logger = logger;
            this.messageBlobTokenDataProtector =
                dataProtector.CreateProtector(MessageBlobTokenPurpose)
                    .ToTimeLimitedDataProtector();
            this.profileBlobTokenDataProtector =
                dataProtector.CreateProtector(ProfileBlobTokenPurpose)
                    .ToTimeLimitedDataProtector();
            this.systemBlobTokenDataProtector =
                dataProtector.CreateProtector(SystemBlobTokenPurpose)
                    .ToTimeLimitedDataProtector();
        }

        private static readonly Type[] MessageBlobTokenValueTypes =
            new[] { typeof(int), typeof(int), typeof(int) };
        public record ParsedMessageBlobToken(
            int ProfileId,
            int MessageId,
            int BlobId);
        public ParsedMessageBlobToken? ParseMessageBlobToken(string token)
        {
            object[] tokenValues;
            try
            {
                tokenValues = ParseBlobToken(
                    this.messageBlobTokenDataProtector,
                    token,
                    MessageBlobTokenValueTypes);
            }
            catch (CryptographicException ex)
            {
                this.logger.LogInformation(ex.Message);
                return null;
            }

            int profileId = (int)tokenValues[0];
            int messageId = (int)tokenValues[1];
            int blobId = (int)tokenValues[2];

            return new ParsedMessageBlobToken(
                profileId,
                messageId,
                blobId);
        }

        private static readonly Type[] ProfileBlobTokenValueTypes =
            new[] { typeof(int), typeof(int) };
        public record ParsedProfileBlobToken(
            int ProfileId,
            int BlobId);
        public ParsedProfileBlobToken? ParseProfileBlobToken(string token)
        {
            object[] tokenValues;
            try
            {
                tokenValues = ParseBlobToken(
                    this.profileBlobTokenDataProtector,
                    token,
                    ProfileBlobTokenValueTypes);
            }
            catch (CryptographicException ex)
            {
                this.logger.LogInformation(ex.Message);
                return null;
            }

            int profileId = (int)tokenValues[0];
            int blobId = (int)tokenValues[1];

            return new ParsedProfileBlobToken(
                profileId,
                blobId);
        }

        private static readonly Type[] SystemBlobTokenValueTypes =
            new[] { typeof(int) };
        public int? ParseSystemBlobToken(string token)
        {
            object[] tokenValues;
            try
            {
                tokenValues = ParseBlobToken(
                    this.systemBlobTokenDataProtector,
                    token,
                    SystemBlobTokenValueTypes);
            }
            catch (CryptographicException ex)
            {
                this.logger.LogInformation(ex.Message);
                return null;
            }

            int blobId = (int)tokenValues[0];

            return blobId;
        }

        private static object[] ParseBlobToken(
            ITimeLimitedDataProtector dataProtector,
            string token,
            Type[] tokenValueTypes)
        {
            byte[] tokenBytes = FromUrlSafeBase64(token);

            byte[] protectedData = dataProtector.Unprotect(tokenBytes, out var _);

            object[] tokenValues =
                CompactPositionalSerializer.Deserialize(
                    protectedData,
                    tokenValueTypes);

            return tokenValues;
        }

        private static byte[] FromUrlSafeBase64(string dataString)
        {
            string incoming = dataString.Replace('_', '/').Replace('-', '+');

            switch (dataString.Length % 4)
            {
                case 2:
                    incoming += "==";
                    break;
                case 3:
                    incoming += "=";
                    break;
            }

            return Convert.FromBase64String(incoming);
        }
    }
}
