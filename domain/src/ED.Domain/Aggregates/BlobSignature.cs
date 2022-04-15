using System;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class BlobSignature
    {
        // EF constructor
        private BlobSignature()
        {
            this.Issuer = null!;
            this.Subject = null!;
            this.SerialNumber = null!;
        }

        public int BlobSignatureId { get; set; }

        public int BlobId { get; set; }

        public byte[]? X509Certificate2DER { get; set; }

        public bool CoversDocument { get; set; }

        public bool CoversPriorRevision { get; set; }

        public DateTime SignDate { get; set; }

        public bool IsTimestamp { get; set; }

        public bool ValidAtTimeOfSigning { get; set; }

        public string Issuer { get; set; }

        public string Subject { get; set; }

        public string SerialNumber { get; set; }

        public int Version { get; set; }

        public DateTime ValidFrom { get; set; }

        public DateTime ValidTo { get; set; }
    }

    class BlobSignatureMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "BlobSignatures";

            var builder = modelBuilder.Entity<BlobSignature>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => e.BlobSignatureId);

            builder.HasOne(typeof(Blob))
                .WithMany()
                .HasForeignKey(nameof(BlobSignature.BlobId))
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
