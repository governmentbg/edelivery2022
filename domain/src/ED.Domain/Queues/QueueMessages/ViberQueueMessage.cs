namespace ED.Domain
{
    public record ViberQueueMessage(
        string? Feature,
        string Recipient,
        string Body,
        object MetaData
    );
}
