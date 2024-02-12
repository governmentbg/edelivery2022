namespace ED.Domain
{
    public partial interface IEsbTicketsSendQueryRepository
    {
        public record GetProfileNamesVO(
            int ProfileId,
            string ProfileName);
    }
}
