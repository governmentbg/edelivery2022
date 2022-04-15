namespace ED.Domain
{
    public partial interface IAdminTemplatesCreateEditViewQueryRepository
    {
        public record ListProfilesVO(
            int ProfileId,
            string ProfileName);
    }
}
