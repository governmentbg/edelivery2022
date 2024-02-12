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
    public class ViberDeliveryCheckJob : QueueJob<ViberDeliveryCheckQueueMessage, DisposableTuple<InfosystemsServiceClient, IServiceScope>>
    {
        private IServiceScopeFactory scopeFactory;

        public ViberDeliveryCheckJob(
           IServiceScopeFactory scopeFactory,
           ILogger<ViberDeliveryCheckJob> logger,
           IOptions<DomainJobsOptions> optionsAccessor)
           : base(QueueMessageType.ViberDeliveryCheck, scopeFactory, logger, optionsAccessor.Value.ViberDeliveryCheckJob)
        {
            this.scopeFactory = scopeFactory;
        }

        protected override Task<DisposableTuple<InfosystemsServiceClient, IServiceScope>> CreateThreadContextAsync(CancellationToken ct)
        {
            var scope = this.scopeFactory.CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<InfosystemsServiceClient>();
            return Task.FromResult(DisposableTuple.Create(client, scope));
        }

        protected override async Task<(QueueJobProcessingResult result, string? error)> HandleMessageAsync(
            DisposableTuple<InfosystemsServiceClient, IServiceScope> context,
            ViberDeliveryCheckQueueMessage payload,
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

                CreateSmsViberDeliveryCommandMessages[] messages =
                    await client.SendViberDeliveryRequest(payload.ViberId, ct);

                using var scope = this.scopeFactory.CreateScope();

                await scope.ServiceProvider
                    .GetRequiredService<IMediator>()
                    .Send(
                        new CreateSmsViberDeliveryCommand(
                            Convert.ToInt32(payload.ViberId),
                            payload.Feature,
                            messages),
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
