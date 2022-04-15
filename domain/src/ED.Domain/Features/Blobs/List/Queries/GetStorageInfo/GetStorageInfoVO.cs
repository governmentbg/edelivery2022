namespace ED.Domain
{
    public partial interface IBlobListQueryRepository
    {
        public record GetStorageInfoVO(
            ulong Quota,
            ulong UsedSpace);
    }
}
