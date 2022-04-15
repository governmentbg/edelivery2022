using System;

namespace EDelivery.WebPortal.Models
{
    public class JWTTokenData
    {
        public string Audience { get; internal set; }

        public int ExpSeconds { get; internal set; }

        public string Issuer { get; internal set; }

        public Guid Jti { get; internal set; }

        public string Key { get; internal set; }

        public string Subject { get; internal set; }

        public virtual JWTPayloadBase GetJWTPayload(
            string encryptedIdentificator,
            string identificatorType,
            double createdTime,
            double expTime)
        {
            return new JWTPayloadBase(
                encryptedIdentificator,
                identificatorType,
                Issuer,
                Subject,
                Audience,
                createdTime,
                expTime,
                Jti);
        }
    }
}