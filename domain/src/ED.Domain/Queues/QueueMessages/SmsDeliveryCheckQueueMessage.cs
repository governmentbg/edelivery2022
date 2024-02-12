namespace ED.Domain
{
    public record SmsDeliveryCheckQueueMessage(
        string? Feature,
        string SmsId
    );
}
