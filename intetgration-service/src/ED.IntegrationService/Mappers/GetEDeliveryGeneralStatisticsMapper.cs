using EDelivery.Common.DataContracts;

namespace ED.IntegrationService
{
    public partial class DataContractMapper
    {
        public static DcStatisticsGeneral ToDcStatisticsGeneral(
            ED.DomainServices.IntegrationService.GetStatisticsResponse resp)
        {
            return new DcStatisticsGeneral
            {
                NumberOfLogins = resp.TotalUsers,
                NumberOfSentMessage = resp.TotalMessages,
                NumberOfSentMessage30days = resp.TotalMessagesLast30Days,
                NumberOfSentMessage10days = resp.TotalMessagesLast10Days,
                NumberOfSentMessageToday = resp.TotalMessagesToday,
                NumberOfRegisteredInstitutions = 0, // never been implemented
                NumberOfRegisteredAdministrations = resp.TargetGroupsCount[3],
                NumberOfRegisteredLegalPerson = resp.TargetGroupsCount[2],
                NumberOfRegisteredSocialOrganisations = resp.TargetGroupsCount[4],
            };
        }
    }
}
