namespace ED.Domain
{
    public partial interface IAdminProfilesCreateEditViewQueryRepository
    {
        public record GetProfileEsbUserInfoVO(
            string? OId,
            string? ClientId);
    }
}
