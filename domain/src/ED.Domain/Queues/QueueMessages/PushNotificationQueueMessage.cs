namespace ED.Domain
{
    public record PushNotificationQueueMessage(
        string? Feature,
        string Recipient,
        object Body,
        object MetaData
    );
}
