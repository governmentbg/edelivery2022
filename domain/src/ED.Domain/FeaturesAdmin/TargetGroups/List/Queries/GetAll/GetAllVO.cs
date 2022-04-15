using System;

namespace ED.Domain
{
    public partial interface IAdminTargetGroupsListQueryRepository
    {
        public record GetAllVO(
            int TargetGroupId,
            string Name,
            DateTime CreateDate,
            DateTime? ArchiveDate);
    }
}
