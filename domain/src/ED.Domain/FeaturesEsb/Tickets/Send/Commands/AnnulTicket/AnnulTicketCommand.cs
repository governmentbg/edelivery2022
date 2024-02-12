using System;
using MediatR;

namespace ED.Domain
{
    public record AnnulTicketCommand(
        int TicketId,
        DateTime AnnulDate,
        string AnnulmentReason,
        int ActionLoginId)
        : IRequest;
}
