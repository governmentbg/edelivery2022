namespace ED.Domain
{
    public record SmsQueueMessage(
        string? Feature,
        string Recipient,
        string Body,
        object MetaData
    );
}
