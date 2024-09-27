namespace ED.Domain
{
    public partial interface IAdminRecipientGroupsCreateEditViewQueryRepository
    {
        public record GetRecipientGroupMembersVO(
            int ProfileId,
            string Name,
            bool HideAsRecipient);
    }
}
