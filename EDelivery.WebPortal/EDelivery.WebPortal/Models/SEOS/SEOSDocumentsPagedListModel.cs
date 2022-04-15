using EDelivery.WebPortal.Enums;

namespace EDelivery.WebPortal.Models
{
    public class SEOSDocumentsPagedListModel
    {
        public PagedList.PagedListLight<SEOSDocumentModel> Documents { get; set; }

        public eSortColumn SortColumn { get; set; }

        public eSortOrder SortOrder { get; set; }
    }
}