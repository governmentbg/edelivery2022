namespace ED.Blobs
{
    public class EncryptorFactoryV2 : IEncryptorFactory
    {
        public IEncryptor CreateEncryptor()
        {
            return new EncryptorV2();
        }

        public IEncryptor CreateEncryptor(byte[] key, byte[] IV)
        {
            return new EncryptorV2(key, IV);
        }
    }
}
