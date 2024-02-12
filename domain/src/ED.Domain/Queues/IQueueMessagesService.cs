using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public interface IQueueMessagesService
    {
        Task<bool> TryPostMessageAndSaveAsync<T>(
            T payload,
            DateTime? dueDate,
            string key,
            CancellationToken ct)
            where T : notnull;

        Task PostMessageAsync<T>(
            T payload,
            CancellationToken ct)
            where T : notnull;

        Task PostMessageAsync<T>(
            T payload,
            DateTime? dueDate,
            CancellationToken ct)
            where T : notnull;

        Task PostMessageAsync<T>(
            T payload,
            string tag,
            CancellationToken ct)
            where T : notnull;

        Task PostMessageAsync<T>(
            T payload,
            DateTime? dueDate,
            string tag,
            CancellationToken ct)
            where T : notnull;

        Task<IEnumerable<(T, DateTime)>> CancelMessages<T>(
            string tag,
            CancellationToken ct)
            where T : notnull;

        Task PostMessagesAsync<T>(
            T[] payload,
            string? tag,
            CancellationToken ct)
            where T : notnull;
    }
}
