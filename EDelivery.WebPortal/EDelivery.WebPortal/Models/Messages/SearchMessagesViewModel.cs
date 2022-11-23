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
            string fromDate,
            string toDate,
            BoxType boxType)
        {
            this.Subject = subject;
            this.Profile = profile;
            this.FromDate = fromDate;
            this.ToDate = toDate;
            this.BoxType = boxType;
        }

        public string Subject { get; set; }

        public string Profile { get; set; }

        public string FromDate { get; set; }

        public string ToDate { get; set; }

        public BoxType BoxType { get; set; }

        public bool HasFilter
        {
            get
            {
                return !string.IsNullOrEmpty(this.Subject)
                    || !string.IsNullOrEmpty(this.Profile)
                    || !string.IsNullOrEmpty(this.FromDate)
                    || !string.IsNullOrEmpty(this.ToDate);
            }
        }
    }
}
