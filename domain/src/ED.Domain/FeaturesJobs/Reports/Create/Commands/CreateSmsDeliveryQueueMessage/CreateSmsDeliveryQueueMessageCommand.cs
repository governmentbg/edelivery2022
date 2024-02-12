using System;
using MediatR;

namespace ED.Domain
{
    public record CreateSmsDeliveryQueueMessageCommand(
        string? Feature,
        string SmsId,
        DateTime DueDate)
        : IRequest;
}
