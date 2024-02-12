using System;
using System.Threading;
using System.Threading.Tasks;
using ED.Domain;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZiggyCreatures.Caching.Fusion;

namespace ED.DomainJobs
{
    public class DeliveredTicketsJob : QueueJob<DeliveredTicketQueueMessage, DisposableTuple<EsbServiceClient, IServiceScope>>
    {
        private const string EsbTokenCache = "EsbTokenCache";

        private IServiceScopeFactory scopeFactory;
        private readonly IFusionCache cache;

        public DeliveredTicketsJob(
           IServiceScopeFactory scopeFactory,
           ILogger<DeliveredTicketsJob> logger,
           IOptions<DomainJobsOptions> optionsAccessor,
           IFusionCache cache)
           : base(QueueMessageType.DeliveredTicket, scopeFactory, logger, optionsAccessor.Value.DeliveredTicketsJob)
        {
            this.scopeFactory = scopeFactory;
            this.cache = cache;
        }

        protected override Task<DisposableTuple<EsbServiceClient, IServiceScope>> CreateThreadContextAsync(CancellationToken ct)
        {
            var scope = this.scopeFactory.CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<EsbServiceClient>();
            return Task.FromResult(DisposableTuple.Create(client, scope));
        }

        protected override async Task<(QueueJobProcessingResult result, string? error)>
            HandleMessageAsync(
                DisposableTuple<EsbServiceClient, IServiceScope> context,
                DeliveredTicketQueueMessage payload,
                bool isLastAttempt,
                CancellationToken ct)
        {
            EsbServiceClient client = context.Item1;

            try
            {
                if (!this.ShouldProcessQueueMessage(payload.Feature))
                {
                    return (QueueJobProcessingResult.Cancel, null);
                }

                string? token = await this.cache.GetOrSetAsync<string?>(
                    EsbTokenCache,
                    async (ctx, ct) =>
                    {
                        string token = await client.GetTokenAsync(
                            EsbServiceClient.EsbScope.TicketScope,
                            headerRepresentedPersonID: null,
                            headerCorrespondentOID: null,
                            headerOperatorID: null,
                            ct);

                        return token;
                    },
                    options: new FusionCacheEntryOptions
                    {
                        Duration = TimeSpan.FromSeconds(30),
                    },
                    token: ct) ?? throw new Exception("Can not obtain access_token");

                _ = await client.SubmitDeliveredTicketAsync(
                    token,
                    payload.IdentifierType,
                    payload.Identifier,
                    payload.TicketId,
                    payload.Timestamp,
                    ct);

                using var scope = this.scopeFactory.CreateScope();

                await scope.ServiceProvider
                    .GetRequiredService<IMediator>()
                    .Send(
                        new CreateTicketDeliveryCommand(
                            payload.TicketId,
                            DeliveryStatus.Delivered),
                        ct);

                return (QueueJobProcessingResult.Success, null);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                if (isLastAttempt)
                {
                    using var scope = this.scopeFactory.CreateScope();

                    await scope.ServiceProvider
                        .GetRequiredService<IMediator>()
                        .Send(
                            new CreateTicketDeliveryCommand(
                                payload.TicketId,
                                DeliveryStatus.Error),
                            ct);
                }

                return (QueueJobProcessingResult.RetryError, ex.Message);
            }
        }
    }
}
