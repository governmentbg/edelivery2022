namespace ED.Domain
{
    public partial interface IAdminTemplatesCreateEditViewQueryRepository
    {
        public record ListRecipientsVO(
            int ProfileId,
            string ProfileName);
    }
}
