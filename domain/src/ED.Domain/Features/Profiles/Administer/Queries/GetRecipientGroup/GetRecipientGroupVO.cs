using System;

namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        public record GetRecipientGroupVO(
            int RecipientGroupId,
            string Name,
            DateTime CreateDate,
            DateTime ModifyDate);
    }
}
