using System;
using System.Threading;
using System.Threading.Tasks;
using ED.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ED.DomainJobs
{
    public class SmsJob : QueueJob<SmsQueueMessage, DisposableTuple<LinkMobilityServiceClient, IServiceScope>>
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

        protected override Task<DisposableTuple<LinkMobilityServiceClient, IServiceScope>> CreateThreadContextAsync(CancellationToken ct)
        {
            var scope = this.scopeFactory.CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<LinkMobilityServiceClient>();
            return Task.FromResult(DisposableTuple.Create(client, scope));
        }

        protected override async Task<(QueueJobProcessingResult result, string? error)>
            HandleMessageAsync(
                DisposableTuple<LinkMobilityServiceClient, IServiceScope> context,
                SmsQueueMessage payload,
                CancellationToken ct)
        {
            var client = context.Item1;

            try
            {
                string? smsId = await client.SendSmsAsync(payload.Recipient, payload.Body, ct);

                using var scope = this.scopeFactory.CreateScope();

                var queueMessagesService = scope.ServiceProvider
                    .GetRequiredService<IQueueMessagesService>();

                await queueMessagesService.TryPostMessageAndSaveAsync(
                    new SmsDeliveryCheckQueueMessage(smsId!),
                    DateTime.Now.AddMinutes(2),
                    null!,
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
