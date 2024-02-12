namespace ED.Domain
{
    public class PostDeliveredTicketRequest
    {
        public IdentifierType IdentifierType { get; set; }

        public string Identifier { get; set; } = null!;

        public int TicketId { get; set; }

        public string Timestamp { get; set; } = null!;
    }
}
