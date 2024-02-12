namespace ED.Domain
{
    public partial interface IEsbTicketsSendQueryRepository
    {
        public record GetProfileContactsVO(
            string Email,
            string Phone);
    }
}
