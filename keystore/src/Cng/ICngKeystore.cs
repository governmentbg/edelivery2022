using System.Security.Cryptography;

namespace ED.Keystore
{
    public delegate ICngKeystore CngKeystoreResolver(string cngProvider);

    public interface ICngKeystore
    {
        CngKey CreateRsaKey(string keyName);

        CngKey OpenRsaKey(string keyName);

        byte[] ExportRsaKey(CngKey key);

        void ImportRsaKey(string keyName, byte[] keyData);

        RSAEncryptionPadding Padding { get; }
    }
}
