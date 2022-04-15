namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        public record GetRecipientGroupsCountVO(
            int NumberOfRecipientGroups);
    }
}
