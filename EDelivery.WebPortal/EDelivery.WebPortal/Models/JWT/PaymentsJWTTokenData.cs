namespace EDelivery.WebPortal.Models
{
    public class PaymentsJWTTokenData : JWTTokenData
    {
        public string Name { get; internal set; }

        public override JWTPayloadBase GetJWTPayload(
            string encryptedIdentificator,
            string identificatorType,
            double createdTime,
            double expTime)
        {
            PaymentsJWTPayload payload = new PaymentsJWTPayload(
                encryptedIdentificator,
                identificatorType,
                Issuer,
                Subject,
                Audience,
                createdTime,
                expTime,
                Jti)
            {
                name = Name
            };

            return payload;
        }
    }
}