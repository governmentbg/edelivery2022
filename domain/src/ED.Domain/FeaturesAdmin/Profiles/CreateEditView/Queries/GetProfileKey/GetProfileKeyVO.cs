namespace ED.Domain
{
    public partial interface IAdminProfilesCreateEditViewQueryRepository
    {
        public record GetProfileKeyVO(
            string Provider,
            string KeyName,
            string OaepPadding);
    }
}
