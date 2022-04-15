using System;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class RegistrationRequest
    {
        // EF constructor
        private RegistrationRequest()
        {
            this.RegistrationEmail = null!;
            this.RegistrationPhone = null!;
        }

        public RegistrationRequest(
            int registeredProfileId,
            string registrationEmail,
            string registrationPhone,
            int blobId,
            int createdBy)
        {
            this.RegisteredProfileId = registeredProfileId;
            this.RegistrationEmail = registrationEmail;
            this.RegistrationPhone = registrationPhone;
            this.BlobId = blobId;
            this.CreatedBy = createdBy;

            this.CreateDate = DateTime.Now;
            this.Status = RegistrationRequestStatus.New;
        }

        public RegistrationRequest(
            int registeredProfileId,
            string registrationEmail,
            string registrationPhone,
            int blobId,
            int adminUserId,
            bool isAdminContext) // to differ from the other constructor (different logic)
        {
            DateTime now = DateTime.Now;

            this.RegisteredProfileId = registeredProfileId;
            this.RegistrationEmail = registrationEmail;
            this.RegistrationPhone = registrationPhone;
            this.BlobId = blobId;
            this.CreatedBy = Login.SystemLoginId;

            this.CreateDate = now;
            this.Status = RegistrationRequestStatus.Confirmed;
            this.ProcessDate = now;
            this.ProcessedByAdminUserId = adminUserId;
        }

        public int RegistrationRequestId { get; set; }

        public int RegisteredProfileId { get; set; }

        public string RegistrationEmail { get; set; }

        public string RegistrationPhone { get; set; }

        public DateTime CreateDate { get; set; }

        public int CreatedBy { get; set; }

        public DateTime? ProcessDate { get; set; }

        public int? ProcessedByAdminUserId { get; set; }

        public RegistrationRequestStatus Status { get; set; }

        public string? Comment { get; set; }

        public int BlobId { get; set; }

        public void Confirm(int adminUserId, string comment)
        {
            this.AssertNewRegistrationRequestStatus();

            this.ProcessDate = DateTime.Now;
            this.ProcessedByAdminUserId = adminUserId;
            this.Status = RegistrationRequestStatus.Confirmed;
            this.Comment = comment;
        }

        public void Reject(int adminUserId, string comment)
        {
            this.AssertNewRegistrationRequestStatus();

            this.ProcessDate = DateTime.Now;
            this.ProcessedByAdminUserId = adminUserId;
            this.Status = RegistrationRequestStatus.Rejected;
            this.Comment = comment;
        }

        private void AssertNewRegistrationRequestStatus()
        {
            if (this.Status != RegistrationRequestStatus.New)
            {
                throw new DomainValidationException("Registration status should be new");
            }
        }
    }

    class RegistrationRequestMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "RegistrationRequests";

            var builder = modelBuilder.Entity<RegistrationRequest>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => e.RegistrationRequestId);
            builder.Property(e => e.RegistrationRequestId).ValueGeneratedOnAdd();

            builder.HasOne(typeof(Blob))
                .WithMany()
                .HasForeignKey(nameof(RegistrationRequest.BlobId))
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(typeof(Login))
                .WithMany()
                .HasForeignKey(nameof(RegistrationRequest.CreatedBy))
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
