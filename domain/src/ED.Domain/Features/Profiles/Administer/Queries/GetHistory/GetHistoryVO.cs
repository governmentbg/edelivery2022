using System;

namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        public record GetHistoryVO(
            int ProfilesHistoryId,
            int ProfileId,
            DateTime ActionDate,
            ProfileHistoryAction Action,
            string? LoginName,
            string? Details,
            string? AdminName,
            string? Ip);
    }
}
