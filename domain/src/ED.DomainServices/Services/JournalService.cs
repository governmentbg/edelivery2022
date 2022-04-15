using System;
using System.Threading.Tasks;
using ED.Domain;
using ED.DomainServices.Journals;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ED.DomainServices
{
    public class JournalService : Journal.JournalBase
    {
        private readonly IServiceProvider serviceProvider;
        public JournalService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public override async Task<Empty> Create(
            CreateRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
               .GetRequiredService<IMediator>()
               .Send(
                   new CreateRegixReportsAuditLogCommand(
                       request.Token,
                       request.Data,
                       request.LoginId,
                       request.ProfileId,
                       request.DateCreated.ToLocalDateTime()),
                   context.CancellationToken);

            return new Empty();
        }
    }
}
