using System;
using System.IO;
using System.Security.Cryptography;

namespace ED.Blobs
{
    public sealed class EncryptorV2 : IEncryptor, IDisposable
    {
        private readonly Aes symmetricAlgorithm;
        public EncryptorV2()
        {
            this.symmetricAlgorithm = Aes.Create();
            this.symmetricAlgorithm.Padding = PaddingMode.PKCS7;
            this.symmetricAlgorithm.Mode = CipherMode.CBC;
            this.symmetricAlgorithm.GenerateKey();
            this.symmetricAlgorithm.GenerateIV();
        }

        public EncryptorV2(byte[] key, byte[] IV)
        {
            this.symmetricAlgorithm = Aes.Create();
            this.symmetricAlgorithm.Padding = PaddingMode.PKCS7;
            this.symmetricAlgorithm.Mode = CipherMode.CBC;
            this.symmetricAlgorithm.Key = key;
            this.symmetricAlgorithm.IV = IV;
        }

        public ICryptoTransform CreateEncryptor()
            => this.symmetricAlgorithm.CreateEncryptor();

        public ICryptoTransform CreateDecryptor()
            => this.symmetricAlgorithm.CreateDecryptor();

        public byte[] Key => this.symmetricAlgorithm.Key;

        public byte[] IV => this.symmetricAlgorithm.IV;

        // TODO: use EncryptCbc in .net 6
        public byte[] Encrypt(byte[] plaintext)
        {
            using MemoryStream destination = new();
            using ICryptoTransform transform = this.symmetricAlgorithm.CreateEncryptor();
            using CryptoStream cryptoStream = new(destination, transform, CryptoStreamMode.Write);

            cryptoStream.Write(plaintext);
            cryptoStream.FlushFinalBlock();

            return destination.ToArray();
        }

        // TODO: use DecryptCbc in .net 6
        public byte[] Decrypt(byte[] ciphertext)
        {
            using MemoryStream destination = new();
            using ICryptoTransform transform = this.symmetricAlgorithm.CreateDecryptor();
            using CryptoStream cryptoStream = new(destination, transform, CryptoStreamMode.Write);

            cryptoStream.Write(ciphertext);
            cryptoStream.FlushFinalBlock();

            return destination.ToArray();
        }

        public void Dispose()
        {
            this.symmetricAlgorithm.Dispose();
        }
    }
}
