using System;

namespace ED.Domain
{
    public record DeliveredTicketQueueMessage(
        string? Feature,
        IdentifierType IdentifierType,
        string Identifier,
        int TicketId,
        DateTime Timestamp
    );

    public enum IdentifierType
    {
        EGN = 1,
        LNCH = 2,
        Other = 3,
    }
}
