namespace ED.Domain
{
    public enum QueueMessageStatus
    {
        Pending = 1,
        Processing = 2,
        Processed = 3,
        Errored = 4,
        Cancelled = 5,
    }
}
