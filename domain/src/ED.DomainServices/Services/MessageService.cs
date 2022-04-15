using System;
using System.Linq;
using System.Threading.Tasks;
using ED.Domain;
using ED.DomainServices.Messages;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Mapster;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ED.DomainServices
{
    public class MessageService : Messages.Message.MessageBase
    {
        private readonly IServiceProvider serviceProvider;

        public MessageService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public override async Task<GetForwardHistoryResponse> GetForwardHistory(
            GetForwardHistoryRequest request,
            ServerCallContext context)
        {
            IMessageListQueryRepository.GetForwardHistoryVO[] history =
                await this.serviceProvider
                    .GetRequiredService<IMessageListQueryRepository>()
                    .GetForwardHistoryAsync(
                        request.MessageId,
                        context.CancellationToken);

            return new GetForwardHistoryResponse
            {
                History =
                {
                    history.ProjectToType<GetForwardHistoryResponse.Types.ForwardHistory>()
                }
            };
        }

        public override async Task<GetNewMessagesCountResponse> GetNewMessagesCount(
            GetNewMessagesCountRequest request,
            ServerCallContext context)
        {
            IMessageListQueryRepository.GetNewMessagesCountVO[] newMessagesCount =
                await this.serviceProvider
                    .GetRequiredService<IMessageListQueryRepository>()
                    .GetNewMessagesCountAsync(
                        request.LoginId,
                        context.CancellationToken);

            return new GetNewMessagesCountResponse
            {
                NewMessagesCount =
                {
                    newMessagesCount.ProjectToType<GetNewMessagesCountResponse.Types.NewMessagesCount>()
                }
            };
        }

        public override async Task<InboxResponse> Inbox(
            BoxRequest request,
            ServerCallContext context)
        {
            TableResultVO<IMessageListQueryRepository.GetInboxVO> inbox =
                await this.serviceProvider
                    .GetRequiredService<IMessageListQueryRepository>()
                    .GetInboxAsync(
                        request.ProfileId,
                        request.Offset,
                        request.Limit,
                        request.TitleQuery,
                        request.ProfileNameQuery,
                        request.FromDate?.ToLocalDateTime(),
                        request.ToDate?.ToLocalDateTime(),
                        request.Orn,
                        request.ReferencedOrn,
                        context.CancellationToken);

            return new InboxResponse
            {
                Length = inbox.Length,
                Result =
                {
                    inbox.Result.ProjectToType<InboxResponse.Types.Message>()
                }
            };
        }

        public override async Task<OutboxResponse> Outbox(
            BoxRequest request,
            ServerCallContext context)
        {
            TableResultVO<IMessageListQueryRepository.GetOutboxVO> outbox =
                await this.serviceProvider
                    .GetRequiredService<IMessageListQueryRepository>()
                    .GetOutboxAsync(
                        request.ProfileId,
                        request.Offset,
                        request.Limit,
                        request.TitleQuery,
                        request.ProfileNameQuery,
                        request.FromDate?.ToLocalDateTime(),
                        request.ToDate?.ToLocalDateTime(),
                        request.Orn,
                        request.ReferencedOrn,
                        context.CancellationToken);

            return new OutboxResponse
            {
                Length = outbox.Length,
                Result =
                {
                    outbox.Result.ProjectToType<OutboxResponse.Types.Message>()
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
                    new OpenMessageCommand(
                        request.MessageId,
                        request.ProfileId,
                        request.LoginId),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<GetBlobTimestampResponse> GetBlobTimestamp(
            GetBlobTimestampRequest request,
            ServerCallContext context)
        {
            IMessageOpenQueryRepository.GetBlobTimestampVO timestamp =
                await this.serviceProvider
                    .GetRequiredService<IMessageOpenQueryRepository>()
                    .GetBlobTimestampAsync(
                        request.MessageId,
                        request.BlobId,
                        context.CancellationToken);

            return timestamp.Adapt<GetBlobTimestampResponse>();
        }

        public override async Task<GetPdfAsRecipientResponse> GetPdfAsRecipient(
            GetPdfAsRecipientRequest request,
            ServerCallContext context)
        {
            IMessageOpenHORepository.GetPdfAsRecipientVO pdf =
                await this.serviceProvider
                    .GetRequiredService<IMessageOpenHORepository>()
                    .GetPdfAsRecipientAsync(
                        request.MessageId,
                        request.ProfileId,
                        context.CancellationToken);

            return pdf.Adapt<GetPdfAsRecipientResponse>();
        }

        public override async Task<GetPdfAsSenderResponse> GetPdfAsSender(
            GetPdfAsSenderRequest request,
            ServerCallContext context)
        {
            IMessageOpenHORepository.GetPdfAsSenderVO pdf =
                await this.serviceProvider
                    .GetRequiredService<IMessageOpenHORepository>()
                    .GetPdfAsSenderAsync(
                        request.MessageId,
                        context.CancellationToken);

            return pdf.Adapt<GetPdfAsSenderResponse>();
        }

        public override async Task<GetSenderProfileResponse> GetSenderProfile(
            GetSenderProfileRequest request,
            ServerCallContext context)
        {
            IMessageOpenQueryRepository.GetSenderProfileVO senderProfile =
                await this.serviceProvider
                    .GetRequiredService<IMessageOpenQueryRepository>()
                    .GetSenderProfileAsync(
                        request.MessageId,
                        context.CancellationToken);

            return senderProfile.Adapt<GetSenderProfileResponse>();
        }

        public override async Task<GetSummaryAsRecipientResponse> GetSummaryAsRecipient(
            GetSummaryAsRecipientRequest request,
            ServerCallContext context)
        {
            IMessageOpenQueryRepository.GetSummaryAsRecipientVO summary =
                await this.serviceProvider
                    .GetRequiredService<IMessageOpenQueryRepository>()
                    .GetSummaryAsRecipientAsync(
                        request.MessageId,
                        request.ProfileId,
                        context.CancellationToken);

            return summary.Adapt<GetSummaryAsRecipientResponse>();
        }

        public override async Task<GetSummaryAsSenderResponse> GetSummaryAsSender(
            GetSummaryAsSenderRequest request,
            ServerCallContext context)
        {
            IMessageOpenQueryRepository.GetSummaryAsSenderVO summary =
                await this.serviceProvider
                    .GetRequiredService<IMessageOpenQueryRepository>()
                    .GetSummaryAsSenderAsync(
                        request.MessageId,
                        context.CancellationToken);

            return summary.Adapt<GetSummaryAsSenderResponse>();
        }

        public override async Task<GetTemplateContentResponse> GetTemplateContent(
            GetTemplateContentRequest request,
            ServerCallContext context)
        {
            IMessageOpenQueryRepository.GetTemplateContentVO template =
                await this.serviceProvider
                    .GetRequiredService<IMessageOpenQueryRepository>()
                    .GetTemplateContentAsync(
                        request.TemplateId,
                        context.CancellationToken);

            return template.Adapt<GetTemplateContentResponse>();
        }

        public override async Task<GetTimestampResponse> GetTimestampNRD(
            GetTimestampRequest request,
            ServerCallContext context)
        {
            IMessageOpenQueryRepository.GetTimestampNrdVO timestamp =
                await this.serviceProvider
                    .GetRequiredService<IMessageOpenQueryRepository>()
                    .GetTimestampNrdAsync(
                        request.MessageId,
                        request.ProfileId,
                        context.CancellationToken);

            return timestamp.Adapt<GetTimestampResponse>();
        }

        public override async Task<GetTimestampResponse> GetTimestampNRO(
            GetTimestampRequest request,
            ServerCallContext context)
        {
            IMessageOpenQueryRepository.GetTimestampNroVO timestamp =
                await this.serviceProvider
                    .GetRequiredService<IMessageOpenQueryRepository>()
                    .GetTimestampNroAsync(
                        request.MessageId,
                        context.CancellationToken);

            return timestamp.Adapt<GetTimestampResponse>();
        }

        public override async Task<GetMessageRecipientsResponse> GetMessageRecipients(
            GetMessageRecipientsRequest request,
            ServerCallContext context)
        {
            IMessageOpenQueryRepository.GetMessageRecipientsVO[] recipients =
                await this.serviceProvider
                    .GetRequiredService<IMessageOpenQueryRepository>()
                    .GetMessageRecipientsAsync(
                        request.MessageId,
                        context.CancellationToken);

            return new GetMessageRecipientsResponse
            {
                Recipients =
                {
                    recipients.ProjectToType<GetMessageRecipientsResponse.Types.Recipient>()
                }
            };
        }

        public override async Task<ReadResponse> Read(
            ReadRequest request,
            ServerCallContext context)
        {
            IMessageOpenHORepository.GetAsRecipientVO message =
                await this.serviceProvider
                    .GetRequiredService<IMessageOpenHORepository>()
                    .GetAsRecipientAsync(
                        request.MessageId,
                        request.ProfileId,
                        context.CancellationToken);

            IMessageOpenHORepository.GetForwardedMessageAsRecipientVO? forwardedMessage = null;

            if (message.ForwardedMessageId.HasValue)
            {
                forwardedMessage = await this.serviceProvider
                    .GetRequiredService<IMessageOpenHORepository>()
                    .GetForwardedMessageAsRecipientAsync(
                        message.ForwardedMessageId.Value,
                        request.ProfileId,
                        context.CancellationToken);
            }

            ReadResponse response = new()
            {
                Message = message.Adapt<ReadResponse.Types.Message>(),
            };

            if (forwardedMessage != null)
            {
                response.ForwardedMessage =
                    forwardedMessage.Adapt<ReadResponse.Types.ForwardedMessage>();
            }

            return response;
        }

        public override async Task<ViewResponse> View(
            ViewRequest request,
            ServerCallContext context)
        {
            IMessageOpenHORepository.GetAsSenderVO message = await this.serviceProvider
                .GetRequiredService<IMessageOpenHORepository>()
                .GetAsSenderAsync(
                    request.MessageId,
                    context.CancellationToken);

            IMessageOpenHORepository.GetAsRecipientVO? forwardedMessage = null;

            if (message.ForwardedMessageId.HasValue)
            {
                forwardedMessage = await this.serviceProvider
                    .GetRequiredService<IMessageOpenHORepository>()
                    .GetAsRecipientAsync(
                        message.ForwardedMessageId.Value,
                        request.ProfileId,
                        context.CancellationToken);
            }

            ViewResponse response = new()
            {
                Message = message.Adapt<ViewResponse.Types.Message>()
            };

            if (forwardedMessage != null)
            {
                response.ForwardedMessage =
                    forwardedMessage.Adapt<ViewResponse.Types.ForwardedMessage>();
            }

            return response;
        }

        public override async Task<Empty> Send(
            SendRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new SendMessageCommand(
                        request.RecipientGroupIds.Distinct().ToArray(),
                        request.RecipientProfileIds.Distinct().ToArray(),
                        request.SenderProfileId,
                        request.SenderLoginId,
                        request.TemplateId,
                        request.Subject,
                        request.ReferencedOrn,
                        request.AdditionalIdentifier,
                        request.Body,
                        request.MetaFields,
                        request.SenderLoginId,
                        request.BlobIds.ToArray(),
                        request.ForwardedMessageId),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<FindIndividualResponse> FindIndividual(
            FindIndividualRequest request,
            ServerCallContext context)
        {
            IMessageSendQueryRepository.FindIndividualVO? individual =
                await this.serviceProvider
                    .GetRequiredService<IMessageSendQueryRepository>()
                    .FindIndividualAsync(
                        request.FirstName,
                        request.LastName,
                        request.Identifier,
                        context.CancellationToken);

            FindIndividualResponse response = new();

            if (individual != null)
            {
                response.Individual =
                    individual.Adapt<FindIndividualResponse.Types.Individual>();
            }

            return response;
        }

        public override async Task<FindLegalEntityResponse> FindLegalEntity(
            FindLegalEntityRequest request,
            ServerCallContext context)
        {
            IMessageSendQueryRepository.FindLegalEntityVO? legalEntity =
                await this.serviceProvider
                    .GetRequiredService<IMessageSendQueryRepository>()
                    .FindLegalEntityAsync(
                        request.Identifier,
                        context.CancellationToken);

            FindLegalEntityResponse response = new();

            if (legalEntity != null)
            {
                response.LegalEntity =
                    legalEntity.Adapt<FindLegalEntityResponse.Types.LegalEntity>();
            }

            return response;
        }

        public override async Task<FindProfilesResponse> FindProfiles(
            FindProfilesRequest request,
            ServerCallContext context)
        {
            TableResultVO<IMessageSendQueryRepository.FindProfilesVO> profiles =
                await this.serviceProvider
                    .GetRequiredService<IMessageSendQueryRepository>()
                    .FindProfilesAsync(
                        request.Term,
                        request.TargetGroupId,
                        request.Offset,
                        request.Limit,
                        context.CancellationToken);

            return new FindProfilesResponse
            {
                Length = profiles.Length,
                Result =
                {
                    profiles.Result.ProjectToType<FindProfilesResponse.Types.ProfileMessage>()
                }
            };
        }

        public async override Task<GetAllowedTemplatesResponse> GetAllowedTemplates(
            GetAllowedTemplatesRequest request,
            ServerCallContext context)
        {
            TableResultVO<IMessageSendQueryRepository.GetAllowedTemplatesVO> templates =
                await this.serviceProvider
                    .GetRequiredService<IMessageSendQueryRepository>()
                    .GetAllowedTemplatesAsync(
                        request.ProfileId,
                        request.LoginId,
                        context.CancellationToken);

            return new GetAllowedTemplatesResponse
            {
                Length = templates.Length,
                Result =
                {
                    templates.Result.ProjectToType<GetAllowedTemplatesResponse.Types.TemplateMessage>()
                }
            };
        }

        public override async Task<GetInstitutionsResponse> GetInstitutions(
            GetInstitutionsRequest request,
            ServerCallContext context)
        {
            TableResultVO<IMessageSendQueryRepository.GetInstitutionsVO> vos =
                await this.serviceProvider
                    .GetRequiredService<IMessageSendQueryRepository>()
                    .GetInstitutionsAsync(
                        request.Term,
                        request.Offset,
                        request.Limit,
                        context.CancellationToken);

            return new GetInstitutionsResponse
            {
                Institutions =
                {
                    vos.Result.ProjectToType<GetInstitutionsResponse.Types.Institution>()
                }
            };
        }

        public override async Task<GetRecipientGroupsResponse> GetRecipientGroups(
            GetRecipientGroupsRequest request,
            ServerCallContext context)
        {
            TableResultVO<IMessageSendQueryRepository.GetRecipientGroupsVO> recipientGroups =
                await this.serviceProvider
                    .GetRequiredService<IMessageSendQueryRepository>()
                    .GetRecipientGroupsAsync(
                        request.ProfileId,
                        context.CancellationToken);

            return new GetRecipientGroupsResponse
            {
                Length = recipientGroups.Length,
                Result =
                {
                    recipientGroups.Result.ProjectToType<GetRecipientGroupsResponse.Types.RecipientGroupMessage>()
                }
            };
        }

        public override async Task<GetReplyResponse> GetReply(
            GetReplyRequest request,
            ServerCallContext context)
        {
            IMessageSendQueryRepository.GetReplyInfoVO replyInfo =
                await this.serviceProvider
                    .GetRequiredService<IMessageSendQueryRepository>()
                    .GetInfoAsync(
                        request.MessageId,
                        context.CancellationToken);

            return replyInfo.Adapt<GetReplyResponse>();
        }

        public override async Task<GetTargetGroupsResponse> GetTargetGroups(
            GetTargetGroupsRequest request,
            ServerCallContext context)
        {
            TableResultVO<IMessageSendQueryRepository.GetTargetGroupsVO> targetGroups =
                await this.serviceProvider
                    .GetRequiredService<IMessageSendQueryRepository>()
                    .GetTargetGroupsAsync(
                        request.ProfileId,
                        context.CancellationToken);

            return new GetTargetGroupsResponse
            {
                Length = targetGroups.Length,
                Result =
                {
                    targetGroups.Result.ProjectToType<GetTargetGroupsResponse.Types.TargetGroupMessage>()
                }
            };
        }

        public override async Task<ExistsTemplateResponse> ExistsTemplate(
            ExistsTemplateRequest request,
            ServerCallContext context)
        {
            bool exists =
                await this.serviceProvider
                    .GetRequiredService<IMessageSendQueryRepository>()
                    .ExistsTemplateAsync(
                        request.TemplateId,
                        context.CancellationToken);

            return new ExistsTemplateResponse
            {
                Exists = exists
            };
        }

        public override async Task<GetForwardMessageInfoResponse> GetForwardMessageInfo(
            GetForwardMessageInfoRequest request,
            ServerCallContext context)
        {
            IMessageSendQueryRepository.GetForwardMessageInfoVO forwardMessageInfo =
                await this.serviceProvider
                    .GetRequiredService<IMessageSendQueryRepository>()
                    .GetForwardMessageInfoAsync(
                        request.MessageId,
                        context.CancellationToken);

            return forwardMessageInfo.Adapt<GetForwardMessageInfoResponse>();
        }
    }
}
