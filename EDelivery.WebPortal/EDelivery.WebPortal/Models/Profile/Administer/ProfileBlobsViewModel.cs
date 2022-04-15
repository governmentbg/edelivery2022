using System;

namespace EDelivery.WebPortal.Models
{
    public class ProfileBlobsViewModel
    {
        public ProfileBlobsViewModel()
        {
        }

        public ProfileBlobsViewModel(
            ED.DomainServices.Profiles.GetBlobsResponse.Types.BlobMessage blob)
        {
            this.BlobId = blob.BlobId;
            this.FileName = blob.FileName;
            this.Description = blob.Description;
            this.CreateDate = blob.CreateDate.ToLocalDateTime();
            this.CreatedBy = blob.CreatedBy;
        }

        public int BlobId { get; set; }

        public string FileName { get; set; }

        public string Description { get; set; }

        public DateTime CreateDate { get; set; }

        public string CreatedBy { get; set; }
    }
}