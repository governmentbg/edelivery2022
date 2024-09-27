using System;

namespace ED.Domain
{
    public partial interface IAdminReportsListQueryRepository
    {
        public record GetTicketsVO(
            DateTime TicketStatDate,
            int ReceivedTicketIndividuals,
            int ReceivedPenalDecreeIndividuals,
            int ReceivedTicketLegalEntites,
            int ReceivedPenalDecreeLegalEntites,
            int InternalServed,
            int ExternalServed,
            int Annulled,
            int EmailNotifications,
            int PhoneNotifications,
            int DeliveredTicketIndividuals,
            int DeliveredPenalDecreeIndividuals,
            int DeliveredTicketLegalEntites,
            int DeliveredPenalDecreeLegalEntites,
            int SentToActiveProfiles,
            int SentToPassiveProfiles);
    }
}
