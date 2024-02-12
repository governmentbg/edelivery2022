using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ED.Domain;
using System.Text.Json;

namespace ED.DomainJobs
{
    public abstract class QueueJob<TMessage, TThreadContext> : PeriodicBackgroundService
        where TThreadContext : IDisposable
    {
        private QueueMessageType type;
        private IServiceScopeFactory scopeFactory;
        private ILogger logger;
        private int batchSize;
        private int maxFailedAttempts;
        private TimeSpan failedAttemptTimeout;
        private int parallelTasks;
        private string[] features;

        protected QueueJob(
            QueueMessageType type,
            IServiceScopeFactory scopeFactory,
            ILogger logger,
            QueueJobOptions options)
            : base(TimeSpan.FromSeconds(options.PeriodInSeconds))
        {
            this.type = type;
            this.logger = logger;
            this.scopeFactory = scopeFactory;
            this.batchSize = options.BatchSize;
            this.maxFailedAttempts = options.MaxFailedAttempts;
            this.failedAttemptTimeout =
                TimeSpan.FromMinutes(options.FailedAttemptTimeoutInMinutes);
            this.parallelTasks = options.ParallelTasks;
            this.features = options.Features;
        }

        protected ILogger Logger => this.logger;

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken,
            CancellationToken stopToken)
        {
            try
            {
                await this.StartBatch(stoppingToken);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                this.logger.Log(LogLevel.Error, "Exception {ex}; Message: {message}", ex, ex.Message);
            }
        }

        private async Task StartBatch(CancellationToken ct)
        {
            using var scope = this.scopeFactory.CreateScope();
            var queueMessagesRepository =
                scope.ServiceProvider
                .GetRequiredService<IQueueMessagesRepository>();

            var pendingTasks = await queueMessagesRepository.FetchNextAsync(
                this.type,
                this.batchSize,
                this.failedAttemptTimeout,
                ct);

            var total = pendingTasks.Count();
            if (total == 0)
            {
                return;
            }

            var pendingTasksQueue = new ConcurrentQueue<QueueMessage>(pendingTasks);
            var counter = new BatchCounter();

            try
            {
                Stopwatch sw = new();
                sw.Start();

                await this.HandleParallel(pendingTasksQueue, counter, ct);

                sw.Stop();

                if (!ct.IsCancellationRequested)
                {
                    this.logger.Log(
                        LogLevel.Information,
                        "Batch finished in {ElapsedMilliseconds}ms - {Successes} succeeded, {Failures} failed and {Cancels} cancels of total {total}.",
                        sw.ElapsedMilliseconds,
                        counter.Successes,
                        counter.Failures,
                        counter.Cancels,
                        total);
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                if (!ct.IsCancellationRequested)
                {
                    this.logger.Log(LogLevel.Error, "Exception {ex}; Message: {message}", ex, ex.Message);
                }
            }

            if (ct.IsCancellationRequested)
            {
                this.logger.Log(
                    LogLevel.Information,
                    "Job was canceled due to a token cancellation request; Batch finished with {Successes} succeeded, {Failures} failed and {Cancels} cancels of total {total}.",
                    counter.Successes,
                    counter.Failures,
                    counter.Cancels,
                    total);
            }

            if (!pendingTasksQueue.IsEmpty)
            {
                var tasks = new List<QueueMessage>();
                // dequeue all items left in case some thread
                // has not been cancelled and is still working
                while (pendingTasksQueue.TryDequeue(out var task))
                {
                    tasks.Add(task);
                }

                // not passing cancellationToken as this operation
                // must succeed even when cancelled
                await queueMessagesRepository.MakePendingAsync(
                    this.type,
                    tasks.Min(m => m.DueDate),
                    tasks.Max(m => m.DueDate),
                    tasks.Select(e => e.QueueMessageId).ToArray(),
                    default);
            }
        }

        private Task HandleParallel(
            ConcurrentQueue<QueueMessage> pendingTasksQueue,
            BatchCounter counter,
            CancellationToken ct)
        {
            int numberOfParallelTasks =
                Math.Min(pendingTasksQueue.Count, this.parallelTasks);
            var parallelTasks = Enumerable.Range(0, numberOfParallelTasks)
                .Select(pt => Task.Run(() => this.HandleAsync(pendingTasksQueue, counter, ct), ct))
                .ToArray();

            return Task.WhenAll(parallelTasks);
        }

        private async Task HandleAsync(
            ConcurrentQueue<QueueMessage> pendingTasksQueue,
            BatchCounter counter,
            CancellationToken ct)
        {
            using var context = await this.CreateThreadContextAsync(ct);
            using var scope = this.scopeFactory.CreateScope();
            var queueMessagesRepository =
                scope.ServiceProvider
                .GetRequiredService<IQueueMessagesRepository>();

            while (!ct.IsCancellationRequested
                && pendingTasksQueue.TryDequeue(out var message))
            {
                try
                {
                    TMessage payload =
                        JsonSerializer.Deserialize<TMessage>(message.Payload)
                        ?? throw new Exception("Payload should not be null");

                    bool isLastAttempt = message.FailedAttempts == this.maxFailedAttempts - 1;

                    (QueueJobProcessingResult result, string? error) =
                        await this.HandleMessageAsync(context, payload, isLastAttempt, ct);

                    switch (result)
                    {
                        case QueueJobProcessingResult.Success:
                            {
                                await queueMessagesRepository.SetStatusProcessedAsync(
                                    message.Type,
                                    message.DueDate,
                                    message.QueueMessageId,
                                    // queue messages always use UTC to prevent
                                    // problems with daylight saving time
                                    DateTime.UtcNow,
                                    // not passing cancellationToken as this
                                    // operation must succeed even when cancelled
                                    default
                                );
                                counter.CountSuccess();
                            }
                            break;

                        case QueueJobProcessingResult.RetryError:
                            {
                                (int failedAttempts, string failedAttemptsErrors) =
                                    this.IncrementFailedAttempts(message, error);
                                await queueMessagesRepository.SetStatusAndFailedAttemptsAsync(
                                    message.Type,
                                    message.DueDate,
                                    message.QueueMessageId,
                                    failedAttempts < this.maxFailedAttempts
                                        ? QueueMessageStatus.Pending
                                        : QueueMessageStatus.Errored,
                                    // queue messages always use UTC to prevent
                                    // problems with daylight saving time
                                    DateTime.UtcNow,
                                    failedAttempts,
                                    failedAttemptsErrors,
                                    // not passing cancellationToken as this
                                    // operation must succeed even when cancelled
                                    default
                                );
                                counter.CountFailure();
                            }
                            break;

                        case QueueJobProcessingResult.Cancel:
                            {
                                await queueMessagesRepository.SetStatusCancelledAsync(
                                    message.Type,
                                    message.DueDate,
                                    message.QueueMessageId,
                                    // queue messages always use UTC to prevent
                                    // problems with daylight saving time
                                    DateTime.UtcNow,
                                    // not passing cancellationToken as this
                                    // operation must succeed even when cancelled
                                    default
                                );
                                counter.CountCancel();
                            }
                            break;

                        default:
                            throw new Exception("Unknown QueueJobProcessingResult");
                    }
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    (int failedAttempts, string failedAttemptsErrors) =
                        this.IncrementFailedAttempts(
                            message,
                            ex.ToString());
                    await queueMessagesRepository.SetStatusAndFailedAttemptsAsync(
                        message.Type,
                        message.DueDate,
                        message.QueueMessageId,
                        QueueMessageStatus.Errored,
                        // queue messages always use UTC to prevent
                        // problems with daylight saving time
                        DateTime.UtcNow,
                        failedAttempts,
                        failedAttemptsErrors,
                        // not passing cancellationToken as this
                        // operation must succeed even when cancelled
                        default
                    );
                    counter.CountFailure();

                    this.logger.Log(LogLevel.Error, "Exception {ex}; Message: {message}", ex, ex.Message);
                }
            }
        }

        private (int failedAttempts, string failedAttemptsErrors) IncrementFailedAttempts(
            QueueMessage message,
            string? error)
        {
            List<string?> failedAttemptsErrorsList;
            if (string.IsNullOrEmpty(message.FailedAttemptsErrors))
            {
                failedAttemptsErrorsList = new();
            }
            else
            {
                failedAttemptsErrorsList =
                    JsonSerializer.Deserialize<List<string?>>(message.FailedAttemptsErrors)
                    ?? throw new Exception("FailedAttemptsErrors should not be null");
            }

            failedAttemptsErrorsList.Add(error);
            string failedAttemptsErrors = JsonSerializer.Serialize(failedAttemptsErrorsList);

            return (
                failedAttempts: message.FailedAttempts + 1,
                failedAttemptsErrors
            );
        }

        protected abstract Task<TThreadContext> CreateThreadContextAsync(CancellationToken ct);
        protected abstract Task<(QueueJobProcessingResult result, string? error)> HandleMessageAsync(
            TThreadContext context,
            TMessage payload,
            bool isLastAttempt,
            CancellationToken ct);

        protected virtual bool ShouldProcessQueueMessage(string? messageFeature)
        {
            if (!this.features.Any() || string.IsNullOrEmpty(messageFeature))
            {
                return true;
            }

            bool match = this.features
                .Where(e => e.Equals(messageFeature, StringComparison.OrdinalIgnoreCase))
                .Any();

            return match;
        }
    }

#pragma warning disable CA1063 // Implement IDisposable Correctly
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
    public class DefaultThreadContext : IDisposable
    {

        public void Dispose()
        {
        }
    }
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
#pragma warning restore CA1063 // Implement IDisposable Correctly

    // a simplified QueueJob implementation for those that do not need a ThreadContext
    public abstract class QueueJob<TMessage> : QueueJob<TMessage, DefaultThreadContext>
    {
        protected QueueJob(
            QueueMessageType type,
            IServiceScopeFactory scopeFactory,
            ILogger logger,
            QueueJobOptions options)
            : base(type, scopeFactory, logger, options)
        {
        }

        protected override Task<DefaultThreadContext> CreateThreadContextAsync(CancellationToken ct)
        {
            return Task.FromResult(new DefaultThreadContext());
        }

        protected override async Task<(QueueJobProcessingResult result, string? error)> HandleMessageAsync(
            DefaultThreadContext context,
            TMessage payload,
            bool isLastAttempt,
            CancellationToken ct)
        {
            return await this.HandleMessageAsync(payload, ct);
        }

        protected abstract Task<(QueueJobProcessingResult result, string? error)> HandleMessageAsync(
            TMessage payload,
            CancellationToken ct);
    }
}
