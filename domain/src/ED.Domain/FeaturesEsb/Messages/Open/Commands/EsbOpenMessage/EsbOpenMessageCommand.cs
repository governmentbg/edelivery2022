using System;
using MediatR;

namespace ED.Domain
{
    public record EsbOpenMessageCommand(
        int MessageId,
        int ProfileId,
        int LoginId)
        : IRequest<EsbOpenMessageCommandResult?>;

    public record EsbOpenMessageCommandResult(
        DateTime DateReceived,
        int LoginId,
        string LoginName);
}
