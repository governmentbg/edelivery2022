using System;
using System.Threading.Tasks;
using ED.Domain;
using ED.DomainServices.Tickets;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Mapster;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ED.DomainServices
{
    public class TicketService : Tickets.Ticket.TicketBase
    {
        private readonly IServiceProvider serviceProvider;

        public TicketService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public override async Task<GetNewTicketsCountResponse> GetNewTicketsCount(
            GetNewTicketsCountRequest request,
            ServerCallContext context)
        {
            ITicketsListQueryRepository.GetNewTicketsCountQO[] newTicketsCount =
                await this.serviceProvider
                    .GetRequiredService<ITicketsListQueryRepository>()
                    .GetNewTicketsCountAsync(
                        request.LoginId,
                        context.CancellationToken);

            return new GetNewTicketsCountResponse
            {
                NewTicketsCount =
                {
                    newTicketsCount.ProjectToType<GetNewTicketsCountResponse.Types.NewTicketsCount>()
                }
            };
        }

        public override async Task<InboxResponse> Inbox(
            BoxRequest request,
            ServerCallContext context)
        {
            TableResultVO<ITicketsListQueryRepository.GetInboxQO> inbox =
                await this.serviceProvider
                    .GetRequiredService<ITicketsListQueryRepository>()
                    .GetInboxAsync(
                        request.ProfileId,
                        request.Offset,
                        request.Limit,
                        request.From?.ToLocalDateTime(),
                        request.To?.ToLocalDateTime(),
                        context.CancellationToken);

            return new InboxResponse
            {
                Length = inbox.Length,
                Result =
                {
                    inbox.Result.ProjectToType<InboxResponse.Types.Ticket>()
                }
            };
        }

        public override async Task<Empty> Open(
            OpenRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new OpenTicketCommand(
                        request.MessageId,
                        request.ProfileId,
                        request.LoginId),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<ReadResponse> Read(
            ReadRequest request,
            ServerCallContext context)
        {
            ITicketsOpenHORepository.GetAsRecipientVO message =
                await this.serviceProvider
                    .GetRequiredService<ITicketsOpenHORepository>()
                    .GetAsRecipientAsync(
                        request.MessageId,
                        request.ProfileId,
                        context.CancellationToken);

            return new ReadResponse
            {
                Message = message.Adapt<ReadResponse.Types.Message>(),
            };
        }

        public override async Task<LoadMultipleObligationsResponse> LoadMultipleObligations(
            LoadMultipleObligationsRequest request,
            ServerCallContext context)
        {
            LoadMultipleObligationsCommandResult result = await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new LoadMultipleObligationsCommand(
                        request.ProfileId),
                    context.CancellationToken);

            return new LoadMultipleObligationsResponse
            {
                Count = result.Count,
                NotFoundMessage = result.NotFoundMessage,
            };
        }

        public override async Task<GetSingleObligationAccessCodeResponse> GetSingleObligationAccessCode(
            GetSingleObligationAccessCodeRequest request,
            ServerCallContext context)
        {
            GetSingleObligationAccessCodeCommandResult result = await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new GetSingleObligationAccessCodeCommand(
                        request.ProfileId,
                        request.DocumentType,
                        request.DocumentIdentifier),
                    context.CancellationToken);

            return new GetSingleObligationAccessCodeResponse
            {
                AccessCode = result.AccessCode,
                NotFoundMessage = result.NotFoundMessage,
            };
        }

        public override async Task<GetTicketTypeAndDocumentIdentifierResponse> GetTicketTypeAndDocumentIdentifier(
            GetTicketTypeAndDocumentIdentifierRequest request,
            ServerCallContext context)
        {
            ITicketsOpenQueryRepository.GetTicketTypeAndDocumentIdentifierVO data =
                await this.serviceProvider
                    .GetRequiredService<ITicketsOpenQueryRepository>()
                    .GetTicketTypeAndDocumentIdentifierAsync(
                        request.MessageId,
                        context.CancellationToken);

            return data.Adapt<GetTicketTypeAndDocumentIdentifierResponse>();
        }
    }
}
