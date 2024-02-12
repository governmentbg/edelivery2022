using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ED.Domain
{
    internal class QueueMessagesService : IQueueMessagesService
    {
        private IQueueMessagesRepository queueMessagesRepository;
        private IServiceScopeFactory scopeFactory;

        public QueueMessagesService(
            IQueueMessagesRepository queueMessagesRepository,
            IServiceScopeFactory scopeFactory)
        {
            this.queueMessagesRepository = queueMessagesRepository;
            this.scopeFactory = scopeFactory;
        }

        public async Task<bool> TryPostMessageAndSaveAsync<T>(
            T payload,
            DateTime? dueDate,
            string key,
            CancellationToken ct)
            where T : notnull
        {
            // use a new scope as we want to immediately save
            // this QueueMessage with a new UnitOfWork
            using var scope = this.scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();
            var queueMessagesRepository =
                scope.ServiceProvider.GetRequiredService<IQueueMessagesRepository>();
            await PostMessageAsyncInt(
                queueMessagesRepository,
                payload,
                dueDate,
                null,
                key,
                ct);

            try
            {
                await unitOfWork.SaveAsync(ct);
                return true;
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqlException sqlEx &&
                    sqlEx.Number == SqlServerErrorCodes.ViolationOfUniqueIndex &&
                    sqlEx.Message.Contains("UQ_QueueMessages_Key"))
                {
                    // someone created the message with that key before us
                    return false;
                }

                throw;
            }
        }

        public Task PostMessageAsync<T>(T payload, CancellationToken ct) where T : notnull
        {
            return PostMessageAsyncInt(this.queueMessagesRepository, payload, null, null, null, ct);
        }

        public Task PostMessageAsync<T>(T payload, DateTime? dueDate, CancellationToken ct) where T : notnull
        {
            return PostMessageAsyncInt(this.queueMessagesRepository, payload, dueDate, null, null, ct);
        }

        public Task PostMessageAsync<T>(T payload, string? tag, CancellationToken ct) where T : notnull
        {
            return PostMessageAsyncInt(this.queueMessagesRepository, payload, null, tag, null, ct);
        }

        public Task PostMessageAsync<T>(T payload, DateTime? dueDate, string? tag, CancellationToken ct) where T : notnull
        {
            return PostMessageAsyncInt(this.queueMessagesRepository, payload, dueDate, tag, null, ct);
        }

        public Task PostMessagesAsync<T>(T[] payload, string? tag, CancellationToken ct) where T : notnull
        {
            return PostMessagesAsyncInt(this.queueMessagesRepository, payload, null, tag, null, ct);
        }

        public async Task<IEnumerable<(T, DateTime)>> CancelMessages<T>(
            string tag,
            CancellationToken ct)
            where T : notnull
        {
            var cancelledMessages =
                await this.queueMessagesRepository.CancelMessages(GetType<T>(), tag, ct);

            return cancelledMessages
                .Select(m => (
                    payload:
                        JsonSerializer.Deserialize<T>(m.Payload)
                        ?? throw new Exception("Payload should not be null"),
                    dueDate: new DateTime(m.DueDate, DateTimeKind.Utc)
                ))
                .ToList();
        }

        private static readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All)
        };

        private static async Task PostMessageAsyncInt<T>(
            IQueueMessagesRepository queueMessagesRepository,
            T payload,
            DateTime? dueDate,
            string? tag,
            string? key,
            CancellationToken ct)
            where T : notnull
        {
            await queueMessagesRepository.AddAsync(
                new QueueMessage(
                    GetType<T>(),
                    JsonSerializer.Serialize(
                        payload,
                        typeof(T),
                        jsonSerializerOptions),
                    dueDate,
                    tag,
                    key),
                ct);
        }

        private static async Task PostMessagesAsyncInt<T>(
            IQueueMessagesRepository queueMessagesRepository,
            T[] payload,
            DateTime? dueDate,
            string? tag,
            string? key,
            CancellationToken ct)
            where T : notnull
        {
            await queueMessagesRepository.AddRangeAsync(
                payload
                    .Select(p => new QueueMessage(
                        GetType<T>(),
                        JsonSerializer.Serialize(
                            p,
                            typeof(T),
                            jsonSerializerOptions),
                        dueDate,
                        tag,
                        key))
                    .ToArray(),
                ct);
        }

        private static QueueMessageType GetType<T>() where T : notnull
        {
            if (typeof(T) == typeof(EmailQueueMessage))
            {
                return QueueMessageType.Email;
            }
            else if (typeof(T) == typeof(SmsQueueMessage))
            {
                return QueueMessageType.Sms;
            }
            else if (typeof(T) == typeof(PushNotificationQueueMessage))
            {
                return QueueMessageType.PushNotification;
            }
            else if (typeof(T) == typeof(ViberQueueMessage))
            {
                return QueueMessageType.Viber;
            }
            else if (typeof(T) == typeof(SmsDeliveryCheckQueueMessage))
            {
                return QueueMessageType.SmsDeliveryCheck;
            }
            else if (typeof(T) == typeof(ViberDeliveryCheckQueueMessage))
            {
                return QueueMessageType.ViberDeliveryCheck;
            }
            else if (typeof(T) == typeof(TranslationQueueMessage))
            {
                return QueueMessageType.Translation;
            }
            else if (typeof(T) == typeof(TranslationClosureQueueMessage))
            {
                return QueueMessageType.TranslationClosure;
            }
            else if (typeof(T) == typeof(DeliveredTicketQueueMessage))
            {
                return QueueMessageType.DeliveredTicket;
            }
            else if (typeof(T) == typeof(DataPortalQueueMessage))
            {
                return QueueMessageType.DataPortal;
            }
            else
            {
                throw new Exception("Unknown payload type.");
            }
        }
    }
}
