namespace ED.Domain
{
    public record SmsQueueMessage(
        string Recipient,
        string Body,
        object MetaData
    );
}
