using EDeliveryResources;

namespace EDelivery.WebPortal.Models
{
    public class ListOutboxBlobsViewModel
    {
        public ListOutboxBlobsViewModel()
        {
            this.SearchFilter = new SearchOutboxBlobsViewModel();
        }

        public SearchOutboxBlobsViewModel SearchFilter { get; set; }

        public PagedList.PagedListLight<ED.DomainServices.Blobs.GetProfileOutboxBlobsResponse.Types.Blob> Blobs { get; set; }

        public string NoBlobsMessage
        {
            get
            {
                return SearchFilter.HasFilter
                    ? StoragePage.LabelNoFilesFromSearch
                    : StoragePage.LabelNoFiles;
            }
        }
    }
}