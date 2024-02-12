using System;
using MediatR;

namespace ED.Domain
{
    public record CreateDataPortalDeliveryQueueMessageCommand(
        string? Feature,
        string DatasetUri,
        DataPortalQueueMessageType Type,
        DateTime DueDate)
        : IRequest;
}
