using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

#pragma warning disable CA1810 // Initialize all static fields when those fields are declared and remove the explicit static constructor
#pragma warning disable CA5379 // Rfc2898DeriveBytes created with a weak hash algorithm
#pragma warning disable CA5387 // Use at least 100000 iterations when deriving a cryptographic key from a password.
namespace ED.Domain
{
    public sealed class EncryptorV1 : IEncryptor, IDisposable
    {
        private const string password = "<secret>";
        private static readonly byte[] salt = Encoding.UTF8.GetBytes("<secret>");
        private const int keySize = 128;
        private const int blockSize = 128;

        private static byte[] key;
        private static byte[] IV;

        static EncryptorV1()
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt);
            key = pbkdf2.GetBytes(keySize / 8);
            IV = pbkdf2.GetBytes(blockSize / 8);
        }

        private Lazy<Aes> symmetricAlgorithm = new(
            () =>
            {
                var aes = Aes.Create();
                aes.KeySize = keySize;
                aes.Key = key;
                aes.IV = IV;
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.CBC;
                return aes;
            },
            isThreadSafe: false);

        public ICryptoTransform CreateEncryptor()
            => this.symmetricAlgorithm.Value.CreateEncryptor();

        public ICryptoTransform CreateDecryptor()
            => this.symmetricAlgorithm.Value.CreateDecryptor();

        byte[] IEncryptor.Key => throw new NotSupportedException();

        byte[] IEncryptor.IV => throw new NotSupportedException();

        // TODO: use EncryptCbc in .net 6
        public byte[] Encrypt(byte[] plaintext)
        {
            using MemoryStream destination = new();
            using ICryptoTransform transform = this.symmetricAlgorithm.Value.CreateEncryptor();
            using CryptoStream cryptoStream = new(destination, transform, CryptoStreamMode.Write);

            cryptoStream.Write(plaintext);
            cryptoStream.FlushFinalBlock();

            return destination.ToArray();
        }

        // TODO: use DecryptCbc in .net 6
        public byte[] Decrypt(byte[] ciphertext)
        {
            using MemoryStream destination = new();
            using ICryptoTransform transform = this.symmetricAlgorithm.Value.CreateDecryptor();
            using CryptoStream cryptoStream = new(destination, transform, CryptoStreamMode.Write);

            cryptoStream.Write(ciphertext);
            cryptoStream.FlushFinalBlock();

            return destination.ToArray();
        }

        public void Dispose()
        {
            if (this.symmetricAlgorithm.IsValueCreated)
            {
                this.symmetricAlgorithm.Value.Dispose();
            }
        }
    }
}
