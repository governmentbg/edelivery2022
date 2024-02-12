using System;
using MediatR;

namespace ED.Domain
{
    public record CreateViberDeliveryQueueMessageCommand(
        string? Feature,
        string ViberId,
        DateTime DueDate)
        : IRequest;
}
