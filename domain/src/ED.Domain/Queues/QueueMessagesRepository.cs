using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.Server;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    class QueueMessagesRepository : IQueueMessagesRepository
    {
        private UnitOfWork unitOfWork;

        public QueueMessagesRepository(UnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task AddAsync(
            QueueMessage queueMessage,
            CancellationToken ct)
        {
            await this.unitOfWork.DbContext.Set<QueueMessage>()
                .AddAsync(queueMessage, ct);
        }

        public async Task AddRangeAsync(
            QueueMessage[] queueMessages,
            CancellationToken ct)
        {
            await this.unitOfWork.DbContext.Set<QueueMessage>()
                .AddRangeAsync(queueMessages, ct);
        }

        public async Task<IEnumerable<QueueMessage>> FetchNextAsync(
            QueueMessageType type,
            int nextCount,
            TimeSpan failedAttemptTimeout,
            CancellationToken ct)
        {
            var maxInterval = DateTime.UtcNow - failedAttemptTimeout;

            var sql =
$@"
WITH m AS (
    SELECT TOP(@nextCount) *
    FROM QueueMessages
    WITH (UPDLOCK, ROWLOCK, READPAST)
    WHERE
        {nameof(QueueMessage.Status)} = {(int)QueueMessageStatus.Pending} AND
        {nameof(QueueMessage.Type)} = @type AND
        {nameof(QueueMessage.DueDate)} < @now AND
        ({nameof(QueueMessage.FailedAttempts)} = 0 OR
            {nameof(QueueMessage.StatusDate)} < @maxInterval)
    ORDER BY
        {nameof(QueueMessage.DueDate)} ASC
)
UPDATE m SET {nameof(QueueMessage.Status)} = {(int)QueueMessageStatus.Processing}
OUTPUT INSERTED.*
";

            return await this.unitOfWork.DbContext.Set<QueueMessage>()
                .FromSqlRaw(sql,
                    new SqlParameter("type", type),
                    new SqlParameter("now", DateTime.UtcNow.Ticks),
                    new SqlParameter("nextCount", nextCount),
                    new SqlParameter("maxInterval", maxInterval))
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task MakePendingAsync(
            QueueMessageType type,
            long startDate,
            long endDate,
            int[] queueMessageIds,
            CancellationToken ct)
        {
            var sql =
$@"
UPDATE QueueMessages SET
    {nameof(QueueMessage.Status)} = {(int)QueueMessageStatus.Pending}
WHERE
    {nameof(QueueMessage.Status)} = {(int)QueueMessageStatus.Processing} AND
    {nameof(QueueMessage.Type)} = @type AND
    {nameof(QueueMessage.DueDate)} BETWEEN @startDate AND @endDate AND
    {nameof(QueueMessage.QueueMessageId)} IN (SELECT Id FROM @ids)
";

            await this.unitOfWork.DbContext.Database
                .ExecuteSqlRawAsync(
                    sql,
                    new[]
                    {
                        new SqlParameter("type", type),
                        new SqlParameter("startDate", startDate),
                        new SqlParameter("endDate", endDate),
                        new SqlParameter("ids", SqlDbType.Structured)
                        {
                            TypeName = "[IdsTableType]",
                            Value = queueMessageIds.Select(id =>
                                {
                                    var record = new SqlDataRecord(metaData: new SqlMetaData("Id", SqlDbType.Int));
                                    record.SetInt32(0, id);
                                    return record;
                                })
                        },
                    },
                    ct);
        }

        public async Task SetStatusProcessedAsync(
            QueueMessageType type,
            long dueDate,
            int queueMessageId,
            DateTime statusDate,
            CancellationToken ct)
        {
            var sql =
$@"
UPDATE QueueMessages SET
    {nameof(QueueMessage.Status)} = {(int)QueueMessageStatus.Processed},
    {nameof(QueueMessage.StatusDate)} = @statusDate
WHERE
    {nameof(QueueMessage.Status)} = {(int)QueueMessageStatus.Processing} AND
    {nameof(QueueMessage.Type)} = @type AND
    {nameof(QueueMessage.DueDate)} = @dueDate AND
    {nameof(QueueMessage.QueueMessageId)} = @queueMessageId
";

            await this.unitOfWork.DbContext.Database
                .ExecuteSqlRawAsync(
                    sql,
                    new[]
                    {
                        new SqlParameter("type", type),
                        new SqlParameter("dueDate", dueDate),
                        new SqlParameter("queueMessageId", queueMessageId),
                        new SqlParameter("statusDate", statusDate),
                    },
                    ct);
        }

        public async Task SetStatusAndFailedAttemptsAsync(
            QueueMessageType type,
            long dueDate,
            int queueMessageId,
            QueueMessageStatus status,
            DateTime statusDate,
            int failedAttempts,
            string failedAttemptsErrors,
            CancellationToken ct)
        {
            var sql =
$@"
UPDATE QueueMessages SET
    {nameof(QueueMessage.Status)} = @status,
    {nameof(QueueMessage.StatusDate)} = @statusDate,
    {nameof(QueueMessage.FailedAttempts)} = @failedAttempts,
    {nameof(QueueMessage.FailedAttemptsErrors)} = @failedAttemptsErrors
WHERE
    {nameof(QueueMessage.Status)} = {(int)QueueMessageStatus.Processing} AND
    {nameof(QueueMessage.Type)} = @type AND
    {nameof(QueueMessage.DueDate)} = @dueDate AND
    {nameof(QueueMessage.QueueMessageId)} = @queueMessageId
";

            await this.unitOfWork.DbContext.Database
                .ExecuteSqlRawAsync(
                    sql,
                    new[]
                    {
                        new SqlParameter("type", type),
                        new SqlParameter("dueDate", dueDate),
                        new SqlParameter("queueMessageId", queueMessageId),
                        new SqlParameter("status", (int)status),
                        new SqlParameter("statusDate", statusDate),
                        new SqlParameter("failedAttempts", failedAttempts),
                        new SqlParameter("failedAttemptsErrors", failedAttemptsErrors),
                    },
                    ct);
        }

        public async Task<IEnumerable<QueueMessage>> CancelMessages(
            QueueMessageType type,
            string tag,
            CancellationToken ct)
        {
            var sql =
$@"
UPDATE QueueMessages SET
    {nameof(QueueMessage.Status)} = {(int)QueueMessageStatus.Cancelled},
    {nameof(QueueMessage.StatusDate)} = @statusDate
OUTPUT INSERTED.*
WHERE
    {nameof(QueueMessage.Status)} IN ({(int)QueueMessageStatus.Pending}, {(int)QueueMessageStatus.Processing}, {(int)QueueMessageStatus.Errored}) AND
    {nameof(QueueMessage.Type)} = @type AND
    {nameof(QueueMessage.Tag)} = @tag
";

            return await this.unitOfWork.DbContext.Set<QueueMessage>()
                .FromSqlRaw(sql,
                    new SqlParameter("type", type),
                    new SqlParameter("tag", tag),
                    new SqlParameter("statusDate", DateTime.UtcNow))
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task SetStatusCancelledAsync(
            QueueMessageType type,
            long dueDate,
            int queueMessageId,
            DateTime statusDate,
            CancellationToken ct)
        {
            var sql =
$@"
UPDATE QueueMessages SET
    {nameof(QueueMessage.Status)} = {(int)QueueMessageStatus.Cancelled},
    {nameof(QueueMessage.StatusDate)} = @statusDate
WHERE
    {nameof(QueueMessage.Status)} = {(int)QueueMessageStatus.Processing} AND
    {nameof(QueueMessage.Type)} = @type AND
    {nameof(QueueMessage.DueDate)} = @dueDate AND
    {nameof(QueueMessage.QueueMessageId)} = @queueMessageId
";

            await this.unitOfWork.DbContext.Database
                .ExecuteSqlRawAsync(
                    sql,
                    new[]
                    {
                        new SqlParameter("type", type),
                        new SqlParameter("dueDate", dueDate),
                        new SqlParameter("queueMessageId", queueMessageId),
                        new SqlParameter("statusDate", statusDate),
                    },
                    ct);
        }
    }
}
