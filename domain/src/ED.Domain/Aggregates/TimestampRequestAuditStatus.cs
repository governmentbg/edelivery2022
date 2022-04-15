namespace ED.Domain
{
    // duplicate of TimestampStatus, used by domain entities
    public enum TimestampRequestAuditStatus
    {
        Success = 1,
        Error = 2,
        NetworkError = 3,
    }
}
