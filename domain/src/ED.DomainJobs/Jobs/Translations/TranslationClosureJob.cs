using System;
using System.Threading;
using System.Threading.Tasks;
using ED.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MediatR;

namespace ED.DomainJobs
{
    public class TranslationClosureJob : QueueJob<TranslationClosureQueueMessage>
    {
        private IServiceScopeFactory scopeFactory;

        public TranslationClosureJob(
           IServiceScopeFactory scopeFactory,
           ILogger<TranslationClosureJob> logger,
           IOptions<DomainJobsOptions> optionsAccessor)
           : base(QueueMessageType.TranslationClosure, scopeFactory, logger, optionsAccessor.Value.TranslationClosureJob)
        {
            this.scopeFactory = scopeFactory;
        }

        protected override async Task<(QueueJobProcessingResult result, string? error)> HandleMessageAsync(
                TranslationClosureQueueMessage payload,
                CancellationToken ct)
        {
            try
            {
                using IServiceScope scope = this.scopeFactory.CreateScope();

                IMediator mediator = scope.ServiceProvider.GetService<IMediator>()!;

                 _ = await mediator.Send(
                    new JobsCloseMessageTranslationRequestsCommand(
                        payload.MessageTranslationId),
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
