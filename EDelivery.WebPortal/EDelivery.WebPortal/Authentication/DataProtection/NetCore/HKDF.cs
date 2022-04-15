using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace EDelivery.WebPortal
{
    public class HKDF
    {
        // copied from https://github.com/dotnet/corefx/pull/42567
        // TODO: should be replaced with the native HKDF.DeriveKey
        // that has shipped with dotnet 5

        /// <summary>
        /// Performs the key derivation HKDF Expand and Extract functions
        /// </summary>
        /// <param name="hashAlgorithmName">The hash algorithm used for HMAC operations.</param>
        /// <param name="ikm">The input keying material.</param>
        /// <param name="outputLength">The length of the output keying material.</param>
        /// <param name="salt">The optional salt value (a non-secret random value). If not provided it defaults to a byte array of <see cref="HashLength"/> zeros.</param>
        /// <param name="info">The optional context and application specific information.</param>
        /// <returns>The output keying material.</returns>
        public static byte[] DeriveKey(HashAlgorithmName hashAlgorithmName, byte[] ikm, int outputLength, byte[] salt = null, byte[] info = null)
        {
            if (ikm == null)
                throw new ArgumentNullException(nameof(ikm));

            int hashLength = HashLength(hashAlgorithmName);
            Debug.Assert(hashLength <= 512 / 8, "hashLength is larger than expected, consider increasing this value or using regular allocation");

            // Constant comes from section 2.3 (the constraint on L in the Inputs section)
            int maxOkmLength = 255 * hashLength;
            if (outputLength > maxOkmLength)
                throw new ArgumentOutOfRangeException(nameof(outputLength), $"Cryptography_Okm_TooLarge maxOkmLength:{maxOkmLength}");

            Span<byte> prk = stackalloc byte[hashLength];

            Extract(hashAlgorithmName, hashLength, ikm, salt, prk);

            byte[] result = new byte[outputLength];
            Expand(hashAlgorithmName, hashLength, prk, result, info);

            return result;
        }

        private static void Extract(HashAlgorithmName hashAlgorithmName, int hashLength, ReadOnlySpan<byte> ikm, ReadOnlySpan<byte> salt, Span<byte> prk)
        {
            Debug.Assert(HashLength(hashAlgorithmName) == hashLength);

            using (IncrementalHash hmac = IncrementalHash.CreateHMAC(hashAlgorithmName, salt.ToArray()))
            {
                hmac.AppendData(ikm.ToArray());
                GetHashAndReset(hmac, prk);
            }
        }

        private static void Expand(HashAlgorithmName hashAlgorithmName, int hashLength, ReadOnlySpan<byte> prk, Span<byte> output, ReadOnlySpan<byte> info)
        {
            Debug.Assert(HashLength(hashAlgorithmName) == hashLength);

            if (prk.Length < hashLength)
                throw new ArgumentException($"Cryptography_Prk_TooSmall hashLength:{hashLength}", nameof(prk));

            Span<byte> counterSpan = stackalloc byte[1];
            ref byte counter = ref counterSpan[0];
            Span<byte> t = Span<byte>.Empty;
            Span<byte> remainingOutput = output;

            using (IncrementalHash hmac = IncrementalHash.CreateHMAC(hashAlgorithmName, prk.ToArray()))
            {
                for (int i = 1; ; i++)
                {
                    hmac.AppendData(t.ToArray());
                    hmac.AppendData(info.ToArray());
                    counter = (byte)i;
                    hmac.AppendData(counterSpan.ToArray());

                    if (remainingOutput.Length >= hashLength)
                    {
                        t = remainingOutput.Slice(0, hashLength);
                        remainingOutput = remainingOutput.Slice(hashLength);
                        GetHashAndReset(hmac, t);
                    }
                    else
                    {
                        if (remainingOutput.Length > 0)
                        {
                            Debug.Assert(hashLength <= 512 / 8, "hashLength is larger than expected, consider increasing this value or using regular allocation");
                            Span<byte> lastChunk = stackalloc byte[hashLength];
                            GetHashAndReset(hmac, lastChunk);
                            lastChunk.Slice(0, remainingOutput.Length).CopyTo(remainingOutput);
                        }

                        break;
                    }
                }
            }
        }

        private static void GetHashAndReset(IncrementalHash hmac, Span<byte> output)
        {
            int bytesWritten;
            try
            {
                byte[] outputArr = hmac.GetHashAndReset();
                bytesWritten = outputArr.Length;
                outputArr.CopyTo(output);
            }
            catch
            {
                Debug.Assert(false, "HMAC operation failed unexpectedly");
                throw new CryptographicException();
            }

            Debug.Assert(bytesWritten == output.Length, $"Bytes written is {bytesWritten} bytes which does not match output length ({output.Length} bytes)");
        }

        private static int HashLength(HashAlgorithmName hashAlgorithmName)
        {
            if (hashAlgorithmName == HashAlgorithmName.SHA1)
            {
                return 160 / 8;
            }
            else if (hashAlgorithmName == HashAlgorithmName.SHA256)
            {
                return 256 / 8;
            }
            else if (hashAlgorithmName == HashAlgorithmName.SHA384)
            {
                return 384 / 8;
            }
            else if (hashAlgorithmName == HashAlgorithmName.SHA512)
            {
                return 512 / 8;
            }
            else if (hashAlgorithmName == HashAlgorithmName.MD5)
            {
                return 128 / 8;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(hashAlgorithmName));
            }
        }
    }
}