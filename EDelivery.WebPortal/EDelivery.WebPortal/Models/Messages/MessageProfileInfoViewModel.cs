namespace EDelivery.WebPortal.Models.Messages
{
    public class MessageProfileInfoViewModel
    {
        public MessageProfileInfoViewModel(
            ED.DomainServices.Messages.GetSenderProfileResponse profile)
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