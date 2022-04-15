namespace ED.Domain
{
    public record PushNotificationQueueMessage(
        string Recipient,
        object Body,
        object MetaData
    );
}
