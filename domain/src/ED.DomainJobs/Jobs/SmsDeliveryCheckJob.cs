using System;
using System.Threading;
using System.Threading.Tasks;
using ED.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ED.DomainJobs
{
    public class SmsDeliveryCheckJob : QueueJob<SmsDeliveryCheckQueueMessage, DisposableTuple<LinkMobilityServiceClient, IServiceScope>>
    {
        private IServiceScopeFactory scopeFactory;

        public SmsDeliveryCheckJob(
           IServiceScopeFactory scopeFactory,
           ILogger<SmsDeliveryCheckJob> logger,
           IOptions<DomainJobsOptions> optionsAccessor)
           : base(QueueMessageType.SmsDeliveryCheck, scopeFactory, logger, optionsAccessor.Value.SmsDeliveryCheckJob)
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
                SmsDeliveryCheckQueueMessage payload,
                CancellationToken ct)
        {
            var client = context.Item1;

            try
            {
                await client.SendSmsDeliveryRequest(payload.SmsId, ct);
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
