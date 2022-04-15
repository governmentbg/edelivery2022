using System;

namespace ED.Domain
{
    public partial interface IAdminRecipientGroupsListQueryRepository
    {
        public record GetAllVO(
            int RecipientGroupId,
            string Name,
            DateTime CreateDate,
            DateTime? ArchiveDate);
    }
}
