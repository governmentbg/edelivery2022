using System;

namespace ED.Domain
{
    public partial interface IAdminRecipientGroupsCreateEditViewQueryRepository
    {
        public record GetRecipientGroupVO(
            int RecipientGroupId,
            string Name,
            DateTime CreateDate,
            DateTime ModifyDate,
            DateTime? ArchiveDate);
    }
}
