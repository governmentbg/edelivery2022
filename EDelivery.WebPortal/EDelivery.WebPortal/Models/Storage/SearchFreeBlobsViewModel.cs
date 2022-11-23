namespace EDelivery.WebPortal.Models
{
    public class SearchFreeBlobsViewModel
    {
        public SearchFreeBlobsViewModel()
        {
        }

        public string FreeBlobsFileName { get; set; }

        public string FreeBlobsAuthor { get; set; }

        public string FreeBlobsFromDate { get; set; }

        public string FreeBlobsToDate { get; set; }
        

        public bool HasFilter
        {
            get
            {
                return !string.IsNullOrEmpty(this.FreeBlobsFileName)
                       || !string.IsNullOrEmpty(this.FreeBlobsAuthor)
                       || !string.IsNullOrEmpty(this.FreeBlobsFromDate)
                       || !string.IsNullOrEmpty(this.FreeBlobsToDate);
            }
        }
    }
}
