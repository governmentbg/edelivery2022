namespace ED.Domain
{
    public record ViberDeliveryCheckQueueMessage(
        string? Feature,
        string ViberId
    );
}
