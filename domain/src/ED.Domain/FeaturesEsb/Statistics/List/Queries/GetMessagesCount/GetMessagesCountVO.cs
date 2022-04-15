namespace ED.Domain
{
    public partial interface IEsbStatisticsListQueryRepository
    {
        public record GetMessagesCountVO(
            int Sent,
            int Received);
    }
}
