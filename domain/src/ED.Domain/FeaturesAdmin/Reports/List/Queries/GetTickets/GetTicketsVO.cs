using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public partial interface IAdminReportsListQueryRepository
    {
        public record GetTicketsVO(
            int TotalTickets,
            int DailyIndividualTickets,
            int DailyLegalEntityTickets,
            int DailyIndividualPenalDecrees,
            int DailyLegalEntityPenalDecrees,
            int DailyNotificationsByEmail,
            int DailyNotificationsByPhone,
            int DailyReceivedIndividualTickets,
            int DailyReceivedLegalEntityTickets,
            int DailyReceivedIndividualPenalDecrees,
            int DailyReceivedLegalEntityPenalDecrees,
            int DailyPassiveProfiles,
            int DailyActiveProfiles);

        [Keyless]
        public record GetDailyTicketsQO(
            string Type,
            int TargetGroupId,
            int MessageCount);
    }
}
