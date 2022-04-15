namespace EDelivery.WebPortal.Models
{
    public class BulSIJWTTokenData : JWTTokenData
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public override JWTPayloadBase GetJWTPayload(
            string encryptedIdentificator,
            string identificatorType,
            double createdTime,
            double expTime)
        {
            BulSIJWTPayload payload = new BulSIJWTPayload(
                encryptedIdentificator,
                identificatorType,
                Issuer,
                Subject,
                Audience,
                createdTime,
                expTime,
                Jti)
            {
                name = Name,
                email = Email,
                phone = Phone
            };

            return payload;
        }
    }
}