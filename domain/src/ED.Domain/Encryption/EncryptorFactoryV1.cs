using System.Diagnostics.CodeAnalysis;

namespace ED.Domain
{
    public class EncryptorFactoryV1 : IEncryptorFactory
    {
        public IEncryptor CreateEncryptor()
        {
            return new EncryptorV1();
        }

        public IEncryptor CreateEncryptor(
            [SuppressMessage("", "CA1801")] byte[] key,
            [SuppressMessage("", "CA1801")] byte[] IV)
        {
            return new EncryptorV1();
        }
    }
}
