using System;
using System.Threading;
using System.Threading.Tasks;
using ED.Domain;
using FluentDate;
using FluentDateTime;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ED.DomainJobs
{
    public class DataPortalJob : QueueJob<DataPortalQueueMessage, DisposableTuple<DataPortalServiceClient, IServiceScope>>
    {
        private IServiceScopeFactory scopeFactory;

        public DataPortalJob(
           IServiceScopeFactory scopeFactory,
           ILogger<DataPortalJob> logger,
           IOptions<DomainJobsOptions> optionsAccessor)
           : base(QueueMessageType.DataPortal, scopeFactory, logger, optionsAccessor.Value.DataPortalJob)
        {
            this.scopeFactory = scopeFactory;
        }

        protected override Task<DisposableTuple<DataPortalServiceClient, IServiceScope>> CreateThreadContextAsync(CancellationToken ct)
        {
            var scope = this.scopeFactory.CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<DataPortalServiceClient>();
            return Task.FromResult(DisposableTuple.Create(client, scope));
        }

        protected override async Task<(QueueJobProcessingResult result, string? error)> HandleMessageAsync(
            DisposableTuple<DataPortalServiceClient, IServiceScope> context,
            DataPortalQueueMessage payload,
            bool isLastAttempt,
            CancellationToken ct)
        {
            DataPortalServiceClient client = context.Item1;

            try
            {
                if (!this.ShouldProcessQueueMessage(payload.Feature))
                {
                    return (QueueJobProcessingResult.Cancel, null);
                }

                using var scope = this.scopeFactory.CreateScope();

                switch (payload.Type)
                {
                    case DataPortalQueueMessageType.ProfilesMonthlyStatistics:

                        DateTime lastMonth = 1.Months().Ago();
                        DateTime from = lastMonth.BeginningOfMonth();
                        DateTime to = lastMonth.EndOfMonth();

                        IJobsDataPortalListQueryRepository.GetProfilesMonthlyStatisticsVO[] statistics =
                            await scope.ServiceProvider
                                .GetRequiredService<IJobsDataPortalListQueryRepository>()
                                .GetProfilesMonthlyStatisticsAsync(
                                    from,
                                    to,
                                    ct);

                        string datasetUri =
                            await client.PostProfilesMonthyStatisticsAsync(
                                payload.DataSetUri,
                                statistics,
                                from,
                                to,
                                ct);

                        DateTime nextMonth = 1.Months().Since(DateTime.Now);

                        await scope.ServiceProvider
                            .GetRequiredService<IMediator>()
                            .Send(
                                new CreateDataPortalDeliveryQueueMessageCommand(
                                    QueueMessageFeatures.DataPortal,
                                    datasetUri,
                                    DataPortalQueueMessageType.ProfilesMonthlyStatistics,
                                    nextMonth),
                                ct);

                        break;
                    default:
                        throw new Exception("Unknown payload type.");
                }

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
