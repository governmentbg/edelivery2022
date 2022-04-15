using EDeliveryResources;

namespace EDelivery.WebPortal.Models
{
    public class ListInboxBlobsViewModel
    {
        public ListInboxBlobsViewModel()
        {
            this.SearchFilter = new SearchInboxBlobsViewModel();
        }

        public SearchInboxBlobsViewModel SearchFilter { get; set; }

        public PagedList.PagedListLight<ED.DomainServices.Blobs.GetProfileInboxBlobsResponse.Types.Blob> Blobs { get; set; }

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