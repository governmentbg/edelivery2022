namespace ED.Domain
{
    public partial interface IEsbBlobsListQueryRepository
    {
        public record CheckStorageBlobVO(bool IsThere);
    }
}
