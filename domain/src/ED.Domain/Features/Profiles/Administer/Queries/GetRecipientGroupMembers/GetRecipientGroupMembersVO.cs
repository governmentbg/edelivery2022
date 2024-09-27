namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        public record GetRecipientGroupMembersVO(
            int ProfileId,
            string ProfileName,
            string ProfileTargetGroup,
            bool HideAsRecipient);
    }
}
