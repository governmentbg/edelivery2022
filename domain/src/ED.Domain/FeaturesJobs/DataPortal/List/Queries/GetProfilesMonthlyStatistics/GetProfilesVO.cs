namespace ED.Domain
{
    public partial interface IJobsDataPortalListQueryRepository
    {
        public record GetProfilesMonthlyStatisticsVO(
            string TargetGroupName,
            string Name,
            int SentMessagesCount,
            int ReceivedMessagesCount);
    }
}
