using System;
using System.Security.Cryptography;

namespace ED.Blobs
{
    public interface IEncryptor : IDisposable
    {
        /// <summary>
        /// The symetric key used for encryption/decryption
        /// </summary>
        byte[] Key { get; }

        /// <summary>
        /// The initialization vector for the encryption/decryption
        /// </summary>
        byte[] IV { get; }

        /// <summary>
        /// Create a new <see cref="ICryptoTransform"/> encryptor.
        /// </summary>
        /// <returns>The encryptor</returns>
        ICryptoTransform CreateEncryptor();

        /// <summary>
        /// Create a new <see cref="ICryptoTransform"/> decryptor.
        /// </summary>
        /// <returns>The decryptor</returns>
        ICryptoTransform CreateDecryptor();

        byte[] Encrypt(byte[] plaintext);

        byte[] Decrypt(byte[] ciphertext);
    }
}
