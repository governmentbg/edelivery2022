using System;

namespace EDelivery.WebPortal.Models
{
    internal class BulSIJWTPayload : JWTPayloadBase
    {
        public string email;
        public string name;
        public string phone;

        public BulSIJWTPayload(
            string encryptedIdentificator,
            string identificatorType,
            string issuer,
            string subject,
            string audience,
            double createdTime,
            double expTime,
            Guid jti)
            : base(
                encryptedIdentificator,
                identificatorType,
                issuer,
                subject,
                audience,
                createdTime,
                expTime,
                jti)
        { }
    }
}