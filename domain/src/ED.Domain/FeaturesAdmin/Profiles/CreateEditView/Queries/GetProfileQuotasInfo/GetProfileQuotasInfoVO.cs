namespace ED.Domain
{
    public partial interface IAdminProfilesCreateEditViewQueryRepository
    {
        public record GetProfileQuotasInfoVO(
            int? StorageQuotaInMb);
    }
}
