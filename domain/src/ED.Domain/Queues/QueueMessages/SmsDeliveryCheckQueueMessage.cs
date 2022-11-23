namespace ED.Domain
{
    public record SmsDeliveryCheckQueueMessage(
        string SmsId
    );
}
