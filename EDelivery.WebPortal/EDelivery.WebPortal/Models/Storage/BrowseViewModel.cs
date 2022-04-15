using System.Collections.Generic;

namespace EDelivery.WebPortal.Models.Storage
{
    public class BrowseViewModel
    {
        public string FileId { get; set; }
        public List<ED.DomainServices.Blobs.GetMyProfileBlobsResponse.Types.Blob> Blobs { get; set; }
        public long MaxFileSize { get; set; }
        public string AllowedFileTypes { get; set; }
    }
}
