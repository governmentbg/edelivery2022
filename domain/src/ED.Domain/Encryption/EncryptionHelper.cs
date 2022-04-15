using System;
using System.Security.Cryptography;
using System.Text;

namespace ED.Domain
{
    public sealed class EncryptionHelper
    {
        public static string GetSha256HashAsHexString(string text)
        {
            return BitConverter.ToString(GetSha256Hash(text)).Replace("-", "");
        }

        private static byte[] GetSha256Hash(string text)
        {
            using SHA256 algorithm = SHA256.Create();

            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(text));
        }

        public static string GetHexString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", String.Empty);
        }

        public static string Sha256Algorithm => nameof(SHA256);
    }
}
