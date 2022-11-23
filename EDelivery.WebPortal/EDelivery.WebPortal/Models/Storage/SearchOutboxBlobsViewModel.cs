namespace EDelivery.WebPortal.Models
{
    public class SearchOutboxBlobsViewModel
    {
        public SearchOutboxBlobsViewModel()
        {
        }

        public string OutboxBlobsFileName { get; set; }

        public string OutboxBlobsMessageSubject { get; set; }

        public string OutboxBlobsFromDate { get; set; }
        
        public string OutboxBlobsToDate { get; set; }
        
        public bool HasFilter
        {
            get
            {
                return !string.IsNullOrEmpty(this.OutboxBlobsFileName)
                       || !string.IsNullOrEmpty(this.OutboxBlobsMessageSubject)
                       || !string.IsNullOrEmpty(this.OutboxBlobsFromDate)
                       || !string.IsNullOrEmpty(this.OutboxBlobsToDate);
            }
        }
    }
}
