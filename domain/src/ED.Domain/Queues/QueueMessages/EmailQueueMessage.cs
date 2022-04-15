namespace ED.Domain
{
    public record EmailQueueMessage(
        string Recipient,
        string Subject,
        string Body,
        bool IsBodyHtml,
        object MetaData
    );
}
