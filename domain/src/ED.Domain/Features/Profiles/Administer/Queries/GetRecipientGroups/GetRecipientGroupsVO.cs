using System;

namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        public record GetRecipientGroupsVO(
            int RecipientGroupId,
            string Name,
            DateTime CreateDate,
            DateTime ModifyDate,
            int NumberOfMembers);
    }
}
