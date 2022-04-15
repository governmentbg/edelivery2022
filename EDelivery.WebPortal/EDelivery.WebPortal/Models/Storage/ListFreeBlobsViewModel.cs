using System.Collections.Generic;

using ED.DomainServices.Blobs;

using EDeliveryResources;

namespace EDelivery.WebPortal.Models
{
    public class ListFreeBlobsViewModel
    {
        public ListFreeBlobsViewModel()
        {
            this.SearchFilter = new SearchFreeBlobsViewModel();
        }

        public ListFreeBlobsViewModel(
            GetProfileFreeBlobsResponse blobsResponse,
            int pageSize,
            int page)
            : this()
        {
            this.Blobs = new PagedList.PagedListLight<GetProfileFreeBlobsResponse.Types.Blob>(
                new List<GetProfileFreeBlobsResponse.Types.Blob>(blobsResponse.Result),
                pageSize,
                page,
                blobsResponse.Length);

            this.StorageQuota = blobsResponse.StorageQuota;
            this.UsedStorageSpace = blobsResponse.UsedStorageSpace;
        }

        public ListFreeBlobsViewModel(
            SearchFreeBlobsViewModel filter,
            GetProfileFreeBlobsResponse blobsResponse,
            int pageSize,
            int page)
        {
            this.SearchFilter = filter;
            this.Blobs = new PagedList.PagedListLight<GetProfileFreeBlobsResponse.Types.Blob>(
                new List<GetProfileFreeBlobsResponse.Types.Blob>(blobsResponse.Result),
                pageSize,
                page,
                blobsResponse.Length);

            this.StorageQuota = blobsResponse.StorageQuota;
            this.UsedStorageSpace = blobsResponse.UsedStorageSpace;
        }

        public SearchFreeBlobsViewModel SearchFilter { get; set; }

        public PagedList.PagedListLight<GetProfileFreeBlobsResponse.Types.Blob> Blobs { get; set; }

        public ulong StorageQuota { get; set; }

        public ulong UsedStorageSpace { get; set; }

        public string QuotaMesage
        {
            get
            {
                return $"{Utils.Utils.FormatSize(this.UsedStorageSpace, 2)}/{Utils.Utils.FormatSize(this.StorageQuota, 2)}";
            }
        }

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
