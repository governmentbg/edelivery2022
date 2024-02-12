using MediatR;

namespace ED.Domain
{
    public record CreateEmailDeliveryCommand(
        DeliveryStatus Status,
        string? Tag)
        : IRequest;
}
