using MediatR;

namespace ED.Domain
{
    public record CreateTicketDeliveryCommand(
        int MessageId,
        DeliveryStatus Status)
        : IRequest;
}
