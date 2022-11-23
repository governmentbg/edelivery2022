namespace ED.IntegrationService
{
    internal class UploadedBlobDO
    {
        public UploadedBlobDO(
            int blobId,
            string fileName,
            string hashAlgorithm,
            string hash,
            ulong size)
        {
            this.BlobId = blobId;
            this.FileName = fileName;
            this.HashAlgorithm = hashAlgorithm;
            this.Hash = hash;
            this.Size = size;
        }

        public int BlobId { get; set; }

        public string FileName { get; set; }

        public string HashAlgorithm { get; set; }

        public string Hash { get; set; }

        public ulong Size { get; set; }
    }
}
