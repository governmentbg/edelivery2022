namespace ED.Domain
{
    public record DataPortalQueueMessage(
        string? Feature,
        string? DataSetUri,
        DataPortalQueueMessageType Type
    );

    public enum DataPortalQueueMessageType
    {
        ProfilesMonthlyStatistics = 1,
    }
}
