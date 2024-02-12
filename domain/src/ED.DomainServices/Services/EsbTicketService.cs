using System;
using System.Linq;
using System.Threading.Tasks;
using ED.Domain;
using ED.DomainServices.Esb;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Mapster;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ED.DomainServices
{
    public class EsbTicketService : Esb.EsbTicket.EsbTicketBase
    {
        private readonly IServiceProvider serviceProvider;

        public EsbTicketService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public override async Task<GetOrCreateRecipientResponse> GetOrCreateRecipient(
            GetOrCreateRecipientRequest request,
            ServerCallContext context)
        {
            GetOrCreateRecipientCommandResult recipient = await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new GetOrCreateRecipientCommand(
                        request.LegalEntityRecipient != null
                            ? new GetOrCreateRecipientCommandLegalEntity(
                                request.LegalEntityRecipient.Identifier)
                            : null,
                        new GetOrCreateRecipientCommandIndividual(
                            request.IndividualRecipient.Identifier,
                            request.IndividualRecipient.FirstName,
                            request.IndividualRecipient.MiddleName,
                            request.IndividualRecipient.LastName,
                            request.IndividualRecipient.Email,
                            request.IndividualRecipient.Phone),
                        request.ActionLoginId,
                        request.Ip),
                    context.CancellationToken);

            return recipient.Adapt<GetOrCreateRecipientResponse>();
        }

        public override async Task<SendTicketResponse> SendTicket(
            SendTicketRequest request,
            ServerCallContext context)
        {
            int messageId = await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new SendTicketCommand(
                        request.IsRecipientIndividual,
                        request.RecipientProfileId,
                        request.RecipientIdentifier,
                        request.SenderProfileId,
                        request.SenderLoginId,
                        Template.TicketTemplate,
                        request.Subject,
                        request.Body,
                        request.Type.Adapt<Domain.TicketType>(),
                        request.DocumentSeries,
                        request.DocumentNumber,
                        request.IssueDate.ToLocalDateTime(),
                        request.VehicleNumber,
                        request.ViolationDate.ToLocalDateTime(),
                        request.ViolatedProvision,
                        request.PenaltyProvision,
                        request.DueAmount,
                        request.DiscountedPaymentAmount,
                        request.Iban,
                        request.Bic,
                        request.PaymentReason,
                        request.NotificationEmail,
                        request.NotificationPhone,
                        new SendTicketCommandBlob(
                            request.Document.FileName,
                            request.Document.HashAlgorithm,
                            request.Document.Hash,
                            request.Document.Size,
                            request.Document.BlobId),
                        request.DocumentIdentifier),
                    context.CancellationToken);

            return new SendTicketResponse
            {
                TicketId = messageId,
            };
        }

        public override async Task<Empty> ServeTicket(
            ServeTicketRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new ServeTicketCommand(
                        request.TicketId,
                        request.ServeDate.ToDateTime(),
                        request.ActionLoginId),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> AnnulTicket(
            AnnulTicketRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new AnnulTicketCommand(
                        request.TicketId,
                        request.AnnulDate.ToDateTime(),
                        request.AnnulmentReason,
                        request.ActionLoginId),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<CheckTicketsResponse> CheckTickets(
            CheckTicketsRequest request,
            ServerCallContext context)
        {
            IEsbTicketsListQueryRepository.TicketsCheckVO[] ticketsCheck =
                await this.serviceProvider
                    .GetRequiredService<IEsbTicketsListQueryRepository>()
                    .TicketsCheckAsync(
                        request.TicketIds.ProjectToType<int>().ToArray(),
                        request.DeliveryStatus.Adapt<int>(),
                        request.From.ToLocalDateTime(),
                        request.To.ToLocalDateTime(),
                        request.ProfileId,
                        context.CancellationToken);

            return new CheckTicketsResponse
            {
                Result =
                {
                    ticketsCheck.ProjectToType<CheckTicketsResponse.Types.TicketCheck>()
                }
            };
        }

        public override async Task<Empty> SendNotification(
            SendNotificationRequest request,
            ServerCallContext context)
        {
            _ = await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new SendNotificationCommand(
                        request.LegalEntityRecipient != null
                            ? new SendNotificationCommandLegalEntity(
                                request.LegalEntityRecipient.Email,
                                request.LegalEntityRecipient.Phone)
                            : null,
                        request.IndividualRecipient != null
                            ? new SendNotificationCommandIndividual(
                                request.IndividualRecipient.Email,
                                request.IndividualRecipient.Phone)
                            : null,
                        request.Email != null
                            ? new SendNotificationCommandEmail(
                                request.Email.Subject,
                                request.Email.Body)
                            : null,
                        request.Sms,
                        request.Viber,
                        request.ProfileId),
                    context.CancellationToken);

            return new Empty();
        }
    }
}
