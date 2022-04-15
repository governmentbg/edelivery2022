using System;
using System.Security.Cryptography;

namespace ED.Keystore
{
    public static class RsaUtils
    {
        public static string FromRSAEncryptionPadding(RSAEncryptionPadding padding)
            => padding switch
            {
                var p when p == RSAEncryptionPadding.OaepSHA1 => "OAEPSHA1",
                var p when p == RSAEncryptionPadding.OaepSHA256 => "OAEPSHA256",
                var p when p == RSAEncryptionPadding.OaepSHA384 => "OAEPSHA384",
                var p when p == RSAEncryptionPadding.OaepSHA512 => "OAEPSHA512",
                _ => throw new Exception("Only OAEP padding is supported."),
            };

        public static RSAEncryptionPadding ToRSAEncryptionPadding(string padding)
            => padding.ToUpperInvariant() switch
            {
                "OAEPSHA1" => RSAEncryptionPadding.OaepSHA1,
                "OAEPSHA256" => RSAEncryptionPadding.OaepSHA256,
                "OAEPSHA384" => RSAEncryptionPadding.OaepSHA384,
                "OAEPSHA512" => RSAEncryptionPadding.OaepSHA512,
                _ => throw new Exception("Only OAEP padding is supported."),
            };
    }
}
