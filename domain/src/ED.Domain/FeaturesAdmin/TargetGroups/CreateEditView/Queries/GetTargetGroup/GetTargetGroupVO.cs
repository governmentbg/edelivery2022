using System;

namespace ED.Domain
{
    public partial interface IAdminTargetGroupsCreateEditViewQueryRepository
    {
        public record GetTargetGroupVO(
            int TargetGroupId,
            string Name,
            DateTime CreateDate,
            DateTime ModifyDate,
            DateTime? ArchiveDate);
    }
}
