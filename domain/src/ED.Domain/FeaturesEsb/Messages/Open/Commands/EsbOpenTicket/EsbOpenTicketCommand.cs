using System;
using MediatR;

namespace ED.Domain
{
    public record EsbOpenTicketCommand(
        int MessageId,
        int ProfileId,
        int LoginId)
        : IRequest<EsbOpenTicketCommandResult>;

    public record EsbOpenTicketCommandResult(
        DateTime DateReceived,
        int LoginId,
        string LoginName);
}
