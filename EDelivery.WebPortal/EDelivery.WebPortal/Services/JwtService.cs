using System;
using System.Collections.Generic;

using EDelivery.WebPortal.Enums;
using EDelivery.WebPortal.Models;
using EDelivery.WebPortal.Models.JWT;
using EDelivery.WebPortal.Utils;
using EDelivery.WebPortal.Utils.Encryption;

using JWT.Algorithms;
using JWT.Builder;

using Newtonsoft.Json;

namespace EDelivery.WebPortal
{
    public class CoordinatorPayload
    {
        public string ProfileId { get; set; }
        public string RedirectUrl { get; set; }
    }

    public class JwtService
    {
        public static CoordinatorPayload ParseCoordinatorJwt(
            string jwt,
            string tokenSecret,
            string aesSecret)
        {
            string payload = JwtBuilder.Create()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(tokenSecret)
                .MustVerifySignature()
                .Decode(jwt);

            CoordinatorPayload details =
                JsonConvert.DeserializeObject<CoordinatorPayload>(payload);

            string decrpyptedIdentifier = AesHelper.Decrypt(
                details.ProfileId,
                Convert.FromBase64String(aesSecret),
                false);

            return new CoordinatorPayload
            {
                ProfileId = decrpyptedIdentifier,
                RedirectUrl = details.RedirectUrl
            };
        }

        /// <summary>
        /// Generate a token, sent in a link to Technologiga in order to get reports 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="personalIdentifier"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetToken<T>(
            int targetGroupId,
            string personalIdentifier,
            T data)
            where T : JWTTokenData
        {
            // to implement a ClockSkew mechanism to account for unsynchronized clocks
            // treat the ExpSeconds as a period of time with NOW in its center
            long createdTime = DateTimeOffset.UtcNow.AddSeconds(-data.ExpSeconds / 2).ToUnixTimeSeconds();
            long expTime = createdTime + data.ExpSeconds;

            string encryptedIdentificator = EncriptionHelper.EncryptStringAES(
                personalIdentifier,
                data.Key);

            string identificatorType = IdentificatorType.EGN;

            if (targetGroupId == (int)TargetGroupId.Individual)
            {
                if (!TextHelper.IsEGN(personalIdentifier) && TextHelper.IsLNCh(personalIdentifier))
                {
                    identificatorType = IdentificatorType.PNF;
                }
            }
            else
            {
                identificatorType = IdentificatorType.EIK;
            }

            Dictionary<string, object> basePayload =
                new Dictionary<string, object>
                {
                    { "iss", data.Issuer },
                    { "sub", string.Format(data.Subject, identificatorType, encryptedIdentificator) },
                    { "aud", data.Audience },
                    { "exp", expTime },
                    { "nbf", createdTime },
                    { "iat", createdTime },
                    { "jti", data.Jti.ToString("N") }
                };

            Dictionary<string, object> uniquePayload = new Dictionary<string, object>();

            if (data is BulSIJWTTokenData bs)
            {
                uniquePayload = new Dictionary<string, object>
                {
                    { "name", bs.Name },
                    { "email", bs.Email },
                    { "phone", bs.Phone }
                };
            }
            else if (data is PaymentsJWTTokenData p)
            {
                uniquePayload = new Dictionary<string, object>
                {
                    { "name", p.Name }
                };
            }

            string token = JwtBuilder.Create()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(data.Key)
                .AddClaims(basePayload)
                .AddClaims(uniquePayload)
                .Encode();

            return token;
        }

        public static T GetJWTDetails<T>(
            string jwt,
            bool validate,
            string sharedSecret)
        {
            try
            {
                T tokenObject = JwtBuilder.Create()
                    .WithAlgorithm(new HMACSHA256Algorithm())
                    .WithSecret(sharedSecret)
                    .WithVerifySignature(validate)
                    .Decode<T>(jwt);

                return tokenObject;
            }
            catch (Exception ex)
            {
                ElmahLogger.Instance.Error(ex, "Get Json Token failed for token " + jwt);

                return default;
            }
        }
    }
}
