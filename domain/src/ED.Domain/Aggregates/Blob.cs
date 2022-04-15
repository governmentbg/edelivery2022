using System;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public class Blob
    {
        // EF constructor
        private Blob()
        {
            this.FileName = null!;
        }

        public int BlobId { get; set; }

        public string FileName { get; set; }

        public string? Hash { get; set; }

        public string? HashAlgorithm { get; set; }

        public byte[]? Timestamp { get; set; }

        public long? Size { get; set; }

        public int? MalwareScanResultId { get; set; }

        public string? DocumentRegistrationNumber { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime ModifyDate { get; set; }

        public byte[] Version { get; set; } = null!;

        public string? FileExtension { get; set; }
    }

    class BlobMapping : EntityMapping
    {
        public override void AddFluentMapping(ModelBuilder modelBuilder)
        {
            var schema = "dbo";
            var tableName = "Blobs";

            var builder = modelBuilder.Entity<Blob>();

            builder.ToTable(tableName, schema);

            builder.HasKey(e => e.BlobId);
            builder.Property(e => e.BlobId).ValueGeneratedOnAdd();
            builder.Property(e => e.FileExtension).HasComputedColumnSql("IIF([FileName] IS NULL, NULL, Right([FileName], CHARINDEX('.', REVERSE([FileName])))) PERSISTED");

            builder.Property(e => e.Version)
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();
        }
    }
}
