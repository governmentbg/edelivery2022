using System;

namespace EDelivery.WebPortal.Models
{
    public class JWTPayloadBase
    {
        /// <summary>
        /// The issuer of the token
        /// </summary>
        public string iss { get; set; }

        /// <summary>
        /// Subject of the token
        /// </summary>
        public string sub { get; set; }

        /// <summary>
        /// Audience
        /// </summary>
        public string aud { get; set; }

        /// <summary>
        ///  This will probably be the registered claim most often used. This will define the expiration in NumericDate value. The expiration MUST be after the current date/time.
        /// </summary>
        public double exp { get; set; }

        /// <summary>
        /// Defines the time before which the JWT MUST NOT be accepted for processing
        /// </summary>
        public double nbf { get; set; }

        /// <summary>
        /// The time the JWT was issued. Can be used to determine the age of the JWT
        /// </summary>
        public double iat { get; set; }

        /// <summary>
        /// Unique identifier for the JWT. Can be used to prevent the JWT from being replayed. This is helpful for a one time use token.
        /// </summary>
        public string jti { get; set; }

        public JWTPayloadBase(
            string identificator,
            string identificatorType,
            string issuer,
            string subject,
            string audience,
            double created,
            double exp,
            Guid jti)
        {
            this.iss = issuer;
            this.sub = string.Format(subject, identificatorType, identificator);
            this.aud = audience;
            this.nbf = this.iat = created;
            this.exp = exp;
            this.jti = jti.ToString("N");
        }
    }
}
