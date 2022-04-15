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
            string ActionLoginName,
            string? Details,
            string? Ip);
    }
}
