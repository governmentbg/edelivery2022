using System;
using System.Threading;
using System.Threading.Tasks;
using ED.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ED.DomainJobs
{
    public class PushNotificationJob : QueueJob<PushNotificationQueueMessage, DisposableTuple<PushNotificationServiceClient, IServiceScope>>
    {
        private IServiceScopeFactory scopeFactory;

        public PushNotificationJob(
           IServiceScopeFactory scopeFactory,
           ILogger<PushNotificationJob> logger,
           IOptions<DomainJobsOptions> optionsAccessor)
           : base(QueueMessageType.PushNotification, scopeFactory, logger, optionsAccessor.Value.PushNotificationJob)
        {
            this.scopeFactory = scopeFactory;
        }

        protected override Task<DisposableTuple<PushNotificationServiceClient, IServiceScope>> CreateThreadContextAsync(CancellationToken ct)
        {
            var scope = this.scopeFactory.CreateScope();
            var pushNotificationServiceClient = scope.ServiceProvider.GetRequiredService<PushNotificationServiceClient>();
            return Task.FromResult(DisposableTuple.Create(pushNotificationServiceClient, scope));
        }
        
        protected override async Task<(QueueJobProcessingResult result, string? error)>
            HandleMessageAsync(
                DisposableTuple<PushNotificationServiceClient, IServiceScope> context,
                PushNotificationQueueMessage payload,
                CancellationToken ct)
        {
            var pushNotificationServiceClient = context.Item1;

            try
            {
                await pushNotificationServiceClient.SendPushNotificationAsync(payload.Recipient, payload.Body, ct);
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
