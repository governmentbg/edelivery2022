namespace ED.Blobs
{
    /// <summary>
    /// An encryptor factory for a specific encryption scheme implementation
    /// </summary>
    public interface IEncryptorFactory
    {
        /// <summary>
        /// Create an encryptor with randomly generated key and IV
        /// </summary>
        /// <returns>The encryptor</returns>
        IEncryptor CreateEncryptor();

        /// <summary>
        /// Create an encryptor with specific key and IV
        /// </summary>
        /// <param name="key">The symetric key used for encryption/decryption</param>
        /// <param name="IV">The initialization vector for the encryption/decryption</param>
        /// <returns>The encryptor</returns>
        IEncryptor CreateEncryptor(byte[] key, byte[] IV);
    }
}
