using MediatR;

namespace ED.Domain
{
    public record CreateSmsViberDeliveryCommand(
        int MsgId,
        string? Tag,
        CreateSmsViberDeliveryCommandMessages[] Messages)
        : IRequest;

    public record CreateSmsViberDeliveryCommandMessages(
        DeliveryStatus Status,
        bool Charge,
        DeliveryResultType Type);

    public enum DeliveryResultType
    {
        Sms = 0,
        Viber = 1,
    }
}
