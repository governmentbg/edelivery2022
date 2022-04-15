namespace ED.Domain
{
    // duplicate of TimestampRequestAuditStatus to keep the timestamp client independent from the domain entites
    public enum TimestampStatus
    {
        Success = 1,
        Error = 2,
        NetworkError = 3,
    }
}
