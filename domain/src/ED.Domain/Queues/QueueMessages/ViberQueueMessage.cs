namespace ED.Domain
{
    public record ViberQueueMessage(
        string Recipient,
        string Body,
        object MetaData
    );
}
