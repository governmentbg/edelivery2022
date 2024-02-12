using System;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public partial interface IMessageListQueryRepository
    {
        [Keyless]
        public record GetOutboxQO(
            int MessageId,
            DateTime DateSent,
            string SenderProfileName,
            string SenderLoginName,
            string Recipients,
            string Subject,
            ForwardStatus ForwardStatusId,
            string TemplateName,
            int NumberOfRecipients,
            int NumberOfTotalRecipients,
            string? Rnu);
    }
}
