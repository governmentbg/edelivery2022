namespace EDelivery.WebPortal.Models
{
    public class PIKRegistrationModel : BaseRegistrationModel
    {
        public string Token { get; set; }
        public string EGN { get; set; }

        public PIKRegistrationModel()
        {
        }

        public PIKRegistrationModel(string token, NoiUserDetails user)
            : base()
        {
            this.Token = token;
            this.FirstName = user.FirstName;
            this.LastName = user.LastName;
            this.MiddleName = user.Surname;
            this.EmailAddress = user.Email;
            this.EGN = user.EGN;
        }

        public string LatinNames { get; set; }
    }
}