﻿namespace ED.Domain
{
    public record EmailQueueMessage(
        string? Feature,
        string Recipient,
        string Subject,
        string Body,
        bool IsBodyHtml,
        object MetaData
    );
}
