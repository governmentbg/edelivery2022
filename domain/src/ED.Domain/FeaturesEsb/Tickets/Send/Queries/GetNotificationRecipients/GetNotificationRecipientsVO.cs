namespace ED.Domain
{
    public partial interface IEsbTicketsSendQueryRepository
    {
        public record GetNotificationRecipientsVO(
            int ProfileId,
            int? LoginId,
            bool IsEmailNotificationEnabled,
            string Email,
            bool IsSmsNotificationEnabled,
            bool IsViberNotificationEnabled,
            string Phone);
    }
}
