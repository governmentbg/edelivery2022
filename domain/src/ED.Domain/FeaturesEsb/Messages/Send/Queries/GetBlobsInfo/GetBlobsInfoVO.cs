namespace ED.Domain
{
    public partial interface IEsbMessagesSendQueryRepository
    {
        public record GetBlobsInfoVO(
            int BlobId,
            string FileName,
            string? HashAlgorithm,
            string? Hash,
            long? Size);
    }
}
