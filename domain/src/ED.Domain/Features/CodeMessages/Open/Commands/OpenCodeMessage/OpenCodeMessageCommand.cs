using System;
using MediatR;

namespace ED.Domain
{
    public record OpenCodeMessageCommand(Guid AccessCode)
        : IRequest<OpenCodeMessageCommandResult>;

    public record OpenCodeMessageCommandResult(
        bool IsSuccessful,
        int? MessageId,
        string Error);
}
