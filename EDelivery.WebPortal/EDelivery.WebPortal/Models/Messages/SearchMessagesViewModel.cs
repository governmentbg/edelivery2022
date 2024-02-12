namespace EDelivery.WebPortal.Models
{
    public enum BoxType
    {
        Inbox = 0,
        Outbox = 1,
    }

    public class SearchMessagesViewModel
    {
        public SearchMessagesViewModel()
        {
        }

        public SearchMessagesViewModel(
            string subject,
            string profile,
            string from,
            string to,
            BoxType boxType)
        {
            this.Subject = subject;
            this.Profile = profile;
            this.From = from;
            this.To = to;
            this.BoxType = boxType;
        }

        public string Subject { get; set; }

        public string Profile { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public BoxType BoxType { get; set; }

        public bool HasFilter
        {
            get
            {
                return !string.IsNullOrEmpty(this.Subject)
                    || !string.IsNullOrEmpty(this.Profile)
                    || !string.IsNullOrEmpty(this.From)
                    || !string.IsNullOrEmpty(this.To);
            }
        }
    }
}
