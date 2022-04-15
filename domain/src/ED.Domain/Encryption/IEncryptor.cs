using System;
using System.Security.Cryptography;

namespace ED.Domain
{
    public interface IEncryptor : IDisposable
    {
        byte[] Key { get; }

        byte[] IV { get; }

        ICryptoTransform CreateEncryptor();

        ICryptoTransform CreateDecryptor();

        byte[] Encrypt(byte[] plaintext);

        byte[] Decrypt(byte[] ciphertext);
    }
}
