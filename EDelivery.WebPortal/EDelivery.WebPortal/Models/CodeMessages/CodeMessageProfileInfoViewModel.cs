namespace EDelivery.WebPortal.Models
{
    public class CodeMessageProfileInfoViewModel
    {
        public CodeMessageProfileInfoViewModel(
            ED.DomainServices.CodeMessages.GetSenderProfileResponse profile)
        {
            this.Name = profile.Name;
            this.Identifier = profile.Identifier;
            this.Phone = profile.Phone;
            this.Email = profile.Email;
            this.Type = profile.Type;
        }

        public string Name { get; set; }

        public string Identifier { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public ED.DomainServices.ProfileType Type { get; internal set; }
    }
}