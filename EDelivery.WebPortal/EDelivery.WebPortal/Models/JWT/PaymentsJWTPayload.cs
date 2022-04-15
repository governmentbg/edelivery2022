using System;

namespace EDelivery.WebPortal.Models
{
    public class PaymentsJWTPayload : JWTPayloadBase
    {
        public string name { get; set; }

        public PaymentsJWTPayload(
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