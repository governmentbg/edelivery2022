using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public interface IQueueMessagesRepository : IRepository
    {
        Task AddAsync(QueueMessage queueMessage, CancellationToken ct);

        Task AddRangeAsync(QueueMessage[] queueMessages, CancellationToken ct);

        Task<IEnumerable<QueueMessage>> FetchNextAsync(
            QueueMessageType type,
            int nextCount,
            TimeSpan failedAttemptTimeout,
            CancellationToken ct);

        Task MakePendingAsync(
            QueueMessageType type,
            long startDate,
            long endDate,
            int[] queueMessageIds,
            CancellationToken ct);

        Task SetStatusProcessedAsync(
            QueueMessageType type,
            long dueDate,
            int queueMessageId,
            DateTime statusDate,
            CancellationToken ct);

        Task SetStatusAndFailedAttemptsAsync(
            QueueMessageType type,
            long dueDate,
            int queueMessageId,
            QueueMessageStatus status,
            DateTime statusDate,
            int failedAttempts,
            string failedAttemptsErrors,
            CancellationToken ct);

        Task<IEnumerable<QueueMessage>> CancelMessages(
            QueueMessageType type,
            string tag,
            CancellationToken ct);

        Task SetStatusCancelledAsync(
            QueueMessageType type,
            long dueDate,
            int queueMessageId,
            DateTime statusDate,
            CancellationToken ct);
    }
}
