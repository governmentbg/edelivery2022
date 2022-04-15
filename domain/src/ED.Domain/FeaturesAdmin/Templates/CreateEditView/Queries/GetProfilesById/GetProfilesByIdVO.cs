namespace ED.Domain
{
    public partial interface IAdminTemplatesCreateEditViewQueryRepository
    {
        public record GetProfilesByIdVO(
            int ProfileId,
            string ProfileName);
    }
}
