using System;
using MediatR;

namespace ED.Domain
{
    public record ServeTicketCommand(
        int TicketId,
        DateTime ServeDate,
        int ActionLoginId)
        : IRequest;
}
