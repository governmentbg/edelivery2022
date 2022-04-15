using System;
using System.Security.Cryptography;

namespace ED.Keystore
{
    public static class RsaEncryptionHelper
    {
        public static byte[] EncryptRsa(
            CngKey key,
            RSAEncryptionPadding padding,
            byte[] plaintextData)
        {
            if (key.Algorithm != CngAlgorithm.Rsa)
            {
                throw new Exception("The specified keyName is not an RSA key");
            }

            if (plaintextData.Length > GetMaxMessageLength(key.KeySize, padding))
            {
                throw new Exception("Message size too large for the specified encryption scheme");
            }

            using RSACng rsaKey = new(key);
            return rsaKey.Encrypt(plaintextData, padding);
        }

        public static byte[] DecryptRsa(
            CngKey key,
            RSAEncryptionPadding padding,
            byte[] encryptedData)
        {
            if (key.Algorithm != CngAlgorithm.Rsa)
            {
                throw new Exception("The specified keyName is not an RSA key");
            }

            using RSACng rsaKey = new(key);
            return rsaKey.Decrypt(encryptedData, padding);
        }

        // In RSA the plaintext message size should be less than the private key(modulus)
        // but to improve RSA's strength we are using padding (OAEP) which further reduces
        // the max message size.
        // see https://crypto.stackexchange.com/a/42100/89659
        private static int GetMaxMessageLength(
            int keySize,
            RSAEncryptionPadding padding)
            => keySize / 8 - 2 * GetRSAEncryptionPaddingHashSize(padding) / 8 - 2;

        private static int GetRSAEncryptionPaddingHashSize(RSAEncryptionPadding padding)
            => padding switch
            {
                var p when p == RSAEncryptionPadding.OaepSHA1 => 160,
                var p when p == RSAEncryptionPadding.OaepSHA256 => 256,
                var p when p == RSAEncryptionPadding.OaepSHA384 => 384,
                var p when p == RSAEncryptionPadding.OaepSHA512 => 512,
                _ => throw new Exception("Only OAEP padding is supported."),
            };
    }
}
