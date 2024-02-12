using System;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public partial interface ITicketsListQueryRepository
    {
        [Keyless]
        public record GetInboxQO(
            int MessageId,
            DateTime DateSent,
            string SenderProfileName,
            string Subject,
            string Type,
            DateTime ViolationDate,
            TicketStatusStatus Status,
            DateTime? SeenDate);
    }
}
