using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace EDelivery.WebPortal.Utils.Encryption
{
    public sealed class AesHelper
    {
        private const byte AesIvSize = 12; // in bytes
        private const byte GcmTagSize = 16; // in bytes
        private const string Algotithm = "AES/GCM/NoPadding";

        private static string PackCipherData(
            byte[] encryptedBytes,
            byte[] iv,
            bool includeSizes)
        {
            int dataSize = encryptedBytes.Length + iv.Length;
            if (includeSizes)
            {
                dataSize += 2; // 2 => ivSize + tagSize
            }

            int index = 0;
            byte[] data = new byte[dataSize];

            if (includeSizes)
            {
                data[index] = AesIvSize;
                index += 1;

                data[index] = GcmTagSize;
                index += 1;
            }

            Array.Copy(iv, 0, data, index, iv.Length);
            index += iv.Length;
            Array.Copy(encryptedBytes, 0, data, index, encryptedBytes.Length);

            return Convert.ToBase64String(data);
        }

        private static (byte[], byte[], byte) UnpackCipherData(
            string cipherText,
            bool includeSizes)
        {
            int index = 0;
            byte[] cipherData = Convert.FromBase64String(cipherText);
            byte ivSize = AesIvSize;
            byte tagSize = GcmTagSize;

            if (includeSizes)
            {
                ivSize = cipherData[index];
                index += 1;

                tagSize = cipherData[index];
                index += 1;
            }

            byte[] iv = new byte[ivSize];
            Array.Copy(cipherData, index, iv, 0, ivSize);
            index += ivSize;

            byte[] encryptedBytes = new byte[cipherData.Length - index];
            Array.Copy(cipherData, index, encryptedBytes, 0, encryptedBytes.Length);
            return (encryptedBytes, iv, tagSize);
        }

        public static string Encrypt(
            string plainText,
            byte[] key,
            bool includeSizes)
        {
            SecureRandom random = new SecureRandom();
            byte[] iv = random.GenerateSeed(AesIvSize);

            ICipherParameters keyParameters =
                new AeadParameters(new KeyParameter(key), GcmTagSize * 8, iv);

            IBufferedCipher cipher = CipherUtilities.GetCipher(Algotithm);
            cipher.Init(true, keyParameters);

            byte[] plainTextData = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherText = cipher.DoFinal(plainTextData);

            return PackCipherData(cipherText, iv, includeSizes);
        }

        public static string Decrypt(
            string cipherText,
            byte[] key,
            bool includeSizes)
        {
            (byte[] encryptedBytes, byte[] iv, byte tagSize) = UnpackCipherData(cipherText, includeSizes);

            ICipherParameters keyParameters =
                new AeadParameters(new KeyParameter(key), GcmTagSize * 8, iv);

            IBufferedCipher cipher = CipherUtilities.GetCipher(Algotithm);
            cipher.Init(false, keyParameters);

            byte[] decryptedData = cipher.DoFinal(encryptedBytes);
            return Encoding.UTF8.GetString(decryptedData);
        }
    }

}
