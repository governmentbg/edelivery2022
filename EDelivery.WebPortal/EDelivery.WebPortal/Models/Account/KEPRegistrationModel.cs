using EDelivery.WebPortal.Utils;

namespace EDelivery.WebPortal.Models
{
    public class KEPRegistrationModel : BaseRegistrationModel
    {
        public CertificateAuthResponse CertInfo { get; set; }

        public KEPRegistrationModel()
        {
        }

        public KEPRegistrationModel(CertificateAuthResponse certResponse)
            : base()
        {
            this.CertInfo = certResponse;
            this.LatinNames = certResponse.LatinNames;
            this.PhoneNumber = certResponse.PhoneNumber;
            this.EmailAddress = certResponse.Email;

            //parse the names
            string[] names = TextHelper.ParseLatinNames(certResponse.LatinNames);
            if (names != null && names.Length == 3)
            {
                this.FirstName = names[0];
                this.MiddleName = names[1];
                this.LastName = names[2];
            }
        }

        public string LatinNames { get; set; }
    }
}
