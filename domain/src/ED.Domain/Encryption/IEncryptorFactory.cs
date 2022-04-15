namespace ED.Domain
{
    public interface IEncryptorFactory
    {
        IEncryptor CreateEncryptor();

        IEncryptor CreateEncryptor(byte[] key, byte[] IV);
    }
}
