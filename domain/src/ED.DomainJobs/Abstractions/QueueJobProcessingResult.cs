namespace ED.DomainJobs
{
    public enum QueueJobProcessingResult
    {
        Success = 1,
        RetryError = 2,
        Cancel = 3,
    }
}
