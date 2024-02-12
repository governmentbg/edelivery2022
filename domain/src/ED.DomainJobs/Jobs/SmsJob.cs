using System;
using System.Threading;
using System.Threading.Tasks;
using ED.Domain;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ED.DomainJobs
{
    public class SmsJob : QueueJob<SmsQueueMessage, DisposableTuple<InfosystemsServiceClient, IServiceScope>>
    {
        private IServiceScopeFactory scopeFactory;

        public SmsJob(
           IServiceScopeFactory scopeFactory,
           ILogger<SmsJob> logger,
           IOptions<DomainJobsOptions> optionsAccessor)
           : base(QueueMessageType.Sms, scopeFactory, logger, optionsAccessor.Value.SmsJob)
        {
            this.scopeFactory = scopeFactory;
        }

        protected override Task<DisposableTuple<InfosystemsServiceClient, IServiceScope>> CreateThreadContextAsync(CancellationToken ct)
        {
            // using scope to inject a new client for each thread
            var scope = this.scopeFactory.CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<InfosystemsServiceClient>();

            // making the scope part of the result so that it is disposed when the thread context is disposed
            return Task.FromResult(DisposableTuple.Create(client, scope));
        }

        protected override async Task<(QueueJobProcessingResult result, string? error)> HandleMessageAsync(
            DisposableTuple<InfosystemsServiceClient, IServiceScope> context,
            SmsQueueMessage payload,
            bool isLastAttempt,
            CancellationToken ct)
        {
            InfosystemsServiceClient client = context.Item1;

            try
            {
                if (!this.ShouldProcessQueueMessage(payload.Feature))
                {
                    return (QueueJobProcessingResult.Cancel, null);
                }

                string? smsId = await client.SendSmsAsync(payload.Recipient, payload.Body, ct);

                using var scope = this.scopeFactory.CreateScope();

                await scope.ServiceProvider
                    .GetRequiredService<IMediator>()
                    .Send(
                        new CreateSmsDeliveryQueueMessageCommand(
                            payload.Feature,
                            smsId!,
                            DateTime.UtcNow.AddMinutes(2)),
                            ct);

                return (QueueJobProcessingResult.Success, null);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                return (QueueJobProcessingResult.RetryError, ex.Message);
            }
        }
    }
}
