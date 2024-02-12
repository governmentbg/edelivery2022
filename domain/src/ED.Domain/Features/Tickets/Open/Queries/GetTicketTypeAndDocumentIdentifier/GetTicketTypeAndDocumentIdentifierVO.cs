namespace ED.Domain
{
    public partial interface ITicketsOpenQueryRepository
    {
        public record GetTicketTypeAndDocumentIdentifierVO(
            string DocumentType,
            string? DocumentIdentifier);
    }
}
