namespace EDelivery.WebPortal.Models
{
    public class SearchInboxBlobsViewModel
    {
        public SearchInboxBlobsViewModel()
        {
        }

        public string InboxBlobsFileName { get; set; }

        public string InboxBlobsMessageSubject { get; set; }

        public string InboxBlobsFromDate { get; set; }
        
        public string InboxBlobsToDate { get; set; }

        public bool HasFilter
        {
            get
            {
                return !string.IsNullOrEmpty(this.InboxBlobsFileName)
                       || !string.IsNullOrEmpty(this.InboxBlobsMessageSubject)
                       || !string.IsNullOrEmpty(this.InboxBlobsFromDate)
                       || !string.IsNullOrEmpty(this.InboxBlobsToDate);
            }
        }
    }
}
