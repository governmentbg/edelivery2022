using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection;

#nullable enable

#pragma warning disable CA5401 // Do not use CreateEncryptor with non-default IV
#pragma warning disable CA5350 // Do Not Use Weak Cryptographic Algorithms
#pragma warning disable SYSLIB0021 // Type or member is obsolete

namespace ED.AdminPanel
{
    internal class SharedSecretDataProtector : IDataProtector
    {
        private static readonly Encoding USAsciiStrict = Encoding.GetEncoding(
            "us-ascii",
            new EncoderExceptionFallback(),
            new DecoderExceptionFallback());

        private object syncRoot = new object();
        private string key;
        private string[] purposes;

        public SharedSecretDataProtector(string key, params string[] purposes)
        {
            this.key = key;
            this.purposes = purposes;
        }

        public IDataProtector CreateProtector(string purpose)
        {
            string[] newPurposes;
            if (this.purposes != null)
            {
                // create a new array with the new purpose at the end
                newPurposes = new string[this.purposes.Length + 1];
                Array.Copy(this.purposes, newPurposes, this.purposes.Length);
                newPurposes[this.purposes.Length] = purpose;
            }
            else
            {
                newPurposes = new[] { purpose };
            }

            return new SharedSecretDataProtector(this.key, newPurposes);
        }

        private (byte[] preamble, byte[] key)? computed;
        private (byte[] preamble, byte[] key) GetComputed()
        {
            if (this.computed == null)
            {
                lock (this.syncRoot)
                {
                    if (this.computed == null)
                    {
                        string purpose = string.Join("", this.purposes);

                        byte[] key = HKDF.DeriveKey(
                            HashAlgorithmName.SHA256,
                            USAsciiStrict.GetBytes(this.key), // throw on characters not in the us-ascii character set
                            256 / 8,
                            info: USAsciiStrict.GetBytes(purpose)); // throw on characters not in the us-ascii character set

                        byte[] preamble;
                        using (var sha = new SHA1Managed())
                        {
                            // use the first 5 bytes of the hashed key + purpose as the preamble
                            preamble = sha.ComputeHash(USAsciiStrict.GetBytes(key + purpose)).Take(5).ToArray();
                        }

                        this.computed = (preamble, key);
                    }
                }
            }

            return this.computed.Value;
        }

        public byte[] Protect(byte[] data)
        {
            (byte[] preamble, byte[] key) = this.GetComputed();

            byte[] dataHash;
            using (var sha = new SHA1Managed())
            {
                dataHash = sha.ComputeHash(data);
            }

            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = key;
                aesAlg.GenerateIV();

                using (var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                using (var msEncrypt = new MemoryStream())
                {
                    msEncrypt.Write(preamble, 0, 5);
                    msEncrypt.Write(aesAlg.IV, 0, 16);

                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (var bwEncrypt = new BinaryWriter(csEncrypt))
                    {
                        bwEncrypt.Write(dataHash);
                        bwEncrypt.Write(data.Length);
                        bwEncrypt.Write(data);
                    }
                    var protectedData = msEncrypt.ToArray();
                    return protectedData;
                }
            }
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            (byte[] preamble, byte[] key) = this.GetComputed();

            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = key;

                using (var msDecrypt = new MemoryStream(protectedData))
                {
                    byte[] p = new byte[5];
                    msDecrypt.Read(p, 0, 5);

                    if (!Enumerable.SequenceEqual(p, preamble))
                    {
                        throw new System.Security.SecurityException("Incorrect preamble!");
                    }

                    byte[] iv = new byte[16];
                    msDecrypt.Read(iv, 0, 16);

                    aesAlg.IV = iv;

                    using (var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var brDecrypt = new BinaryReader(csDecrypt))
                    {
                        var signature = brDecrypt.ReadBytes(20);
                        var len = brDecrypt.ReadInt32();
                        var data = brDecrypt.ReadBytes(len);

                        byte[] dataHash;
                        using (var sha = new SHA1Managed())
                        {
                            dataHash = sha.ComputeHash(data);
                        }

                        if (!dataHash.SequenceEqual(signature))
                            throw new System.Security.SecurityException("Signature does not match the computed hash");

                        return data;
                    }
                }
            }
        }
    }
}
