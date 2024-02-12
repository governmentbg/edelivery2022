using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ED.Domain;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ED.DomainJobs
{
    public class ViberJob : QueueJob<ViberQueueMessage, DisposableTuple<InfosystemsServiceClient, IServiceScope>>
    {
        private IServiceScopeFactory scopeFactory;

        public ViberJob(
           IServiceScopeFactory scopeFactory,
           ILogger<ViberJob> logger,
           IOptions<DomainJobsOptions> optionsAccessor)
           : base(QueueMessageType.Viber, scopeFactory, logger, optionsAccessor.Value.ViberJob)
        {
            this.scopeFactory = scopeFactory;
        }

        protected override Task<DisposableTuple<InfosystemsServiceClient, IServiceScope>> CreateThreadContextAsync(CancellationToken ct)
        {
            var scope = this.scopeFactory.CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<InfosystemsServiceClient>();
            return Task.FromResult(DisposableTuple.Create(client, scope));
        }

        protected override async Task<(QueueJobProcessingResult result, string? error)>
            HandleMessageAsync(
                DisposableTuple<InfosystemsServiceClient, IServiceScope> context,
                ViberQueueMessage payload,
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

                string? fallbackSmsBody = null;
                if (((JsonElement)payload.MetaData).TryGetProperty("FallbackSmsBody", out JsonElement jsonElement))
                {
                    fallbackSmsBody = jsonElement.Deserialize<string?>();
                }

                string? viberId = await client.SendViberAsync(payload.Recipient, payload.Body, fallbackSmsBody, ct);

                using var scope = this.scopeFactory.CreateScope();

                await scope.ServiceProvider
                    .GetRequiredService<IMediator>()
                    .Send(
                        new CreateViberDeliveryQueueMessageCommand(
                            payload.Feature,
                            viberId!,
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
