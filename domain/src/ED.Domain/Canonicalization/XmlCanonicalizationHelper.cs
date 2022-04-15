using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;

namespace ED.Domain
{
    public sealed class XmlCanonicalizationHelper
    {
        public static byte[] GetSha256Hash(Stream xmlStream)
        {
            XmlDsigC14NTransform transform = new(true);
            transform.LoadInput(xmlStream);

            using SHA256 algorithm = SHA256.Create();

            byte[] digestedOutput = transform.GetDigestedOutput(algorithm);

            return digestedOutput;
        }
    }
}
