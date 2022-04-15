using System;
using System.Linq;
using System.Threading.Tasks;
using ED.Domain;
using ED.DomainServices.IntegrationService;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Mapster;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ED.DomainServices
{
    public class IntegrationServiceService : IntegrationService.IntegrationService.IntegrationServiceBase
    {
        private readonly IServiceProvider serviceProvider;

        public IntegrationServiceService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public override async Task<GetRegisteredInstitutionsResponse> GetRegisteredInstitutions(
            Empty request,
            ServerCallContext context)
        {
            IIntegrationServiceProfilesListQueryRepository.GetRegisteredInstitutionsVO[] registeredInstitutions =
                await this.serviceProvider
                    .GetRequiredService<IIntegrationServiceProfilesListQueryRepository>()
                    .GetRegisteredInstitutionsAsync(context.CancellationToken);

            return new GetRegisteredInstitutionsResponse
            {
                Institutions =
                {
                    registeredInstitutions.ProjectToType<GetRegisteredInstitutionsResponse.Types.Institution>()
                }
            };
        }

        public override async Task<HasLoginWithCertificateThumbprintResponse> HasLoginWithCertificateThumbprint(
            HasLoginWithCertificateThumbprintRequest request,
            ServerCallContext context)
        {
            bool hasLogin =
                await this.serviceProvider
                    .GetRequiredService<IIntegrationServiceProfilesListQueryRepository>()
                    .HasLoginWithCertificateThumbprintAsync(
                        request.CertificateThumbprint,
                        context.CancellationToken);

            return new HasLoginWithCertificateThumbprintResponse
            {
                HasLogin = hasLogin
            };
        }

        public override async Task<GetLoginByIdentifierResponse> GetLoginByIdentifier(
            GetLoginByIdentifierRequest request,
            ServerCallContext context)
        {
            IIntegrationServiceProfilesListQueryRepository.GetLoginByIdentifierVO? login =
                await this.serviceProvider
                    .GetRequiredService<IIntegrationServiceProfilesListQueryRepository>()
                    .GetLoginByIdentifierAsync(
                        request.Identifier,
                        context.CancellationToken);

            GetLoginByIdentifierResponse resp = new();

            if (login != null)
            {
                resp.Login = login.Adapt<GetLoginByIdentifierResponse.Types.Login>();
            }

            return resp;
        }

        public override async Task<OutboxResponse> Outbox(
            OutboxRequest request,
            ServerCallContext context)
        {
            TableResultVO<IIntegrationServiceMessagesListQueryRepository.GetOutboxVO> outbox =
                await this.serviceProvider
                    .GetRequiredService<IIntegrationServiceMessagesListQueryRepository>()
                    .GetOutboxAsync(
                        request.CertificateThumbprint,
                        request.Offset,
                        request.Limit,
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

        public override async Task<InboxResponse> Inbox(
            InboxRequest request,
            ServerCallContext context)
        {
            TableResultVO<IIntegrationServiceMessagesListQueryRepository.GetInboxVO> inbox =
                await this.serviceProvider
                    .GetRequiredService<IIntegrationServiceMessagesListQueryRepository>()
                    .GetInboxAsync(
                        request.CertificateThumbprint,
                        request.ShowOnlyNew,
                        request.Offset,
                        request.Limit,
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

        public override async Task<CheckIndividualRegistrationResponse> CheckIndividualRegistration(
            CheckIndividualRegistrationRequest request,
            ServerCallContext context)
        {
            IIntegrationServiceProfilesListQueryRepository.CheckIndividualRegistrationVO checkIndividual =
                await this.serviceProvider
                    .GetRequiredService<IIntegrationServiceProfilesListQueryRepository>()
                    .CheckIndividualRegistrationAsync(
                        request.Identifier,
                        context.CancellationToken);

            return checkIndividual.Adapt<CheckIndividualRegistrationResponse>();
        }

        public override async Task<CheckLegalEntityRegistrationResponse> CheckLegalEntityRegistration(
            CheckLegalEntityRegistrationRequest request,
            ServerCallContext context)
        {
            IIntegrationServiceProfilesListQueryRepository.CheckLegalEntityRegistrationVO checkLegalEntity =
                 await this.serviceProvider
                    .GetRequiredService<IIntegrationServiceProfilesListQueryRepository>()
                    .CheckLegalEntityRegistrationAsync(
                        request.Identifier,
                        context.CancellationToken);

            return checkLegalEntity.Adapt<CheckLegalEntityRegistrationResponse>();
        }

        public override async Task<CheckProfileRegistrationResponse> CheckProfileRegistration(
            CheckProfileRegistrationRequest request,
            ServerCallContext context)
        {
            IIntegrationServiceProfilesListQueryRepository.CheckProfileRegistrationVO checkProfile =
                 await this.serviceProvider
                    .GetRequiredService<IIntegrationServiceProfilesListQueryRepository>()
                    .CheckProfileRegistrationAsync(
                        request.Identifier,
                        context.CancellationToken);

            return checkProfile.Adapt<CheckProfileRegistrationResponse>();
        }

        public override async Task<GetProfileInfoResponse> GetProfileInfo(
            GetProfileInfoRequest request,
            ServerCallContext context)
        {
            IIntegrationServiceProfilesListQueryRepository.GetProfileInfoVO profileInfo =
                await this.serviceProvider
                    .GetRequiredService<IIntegrationServiceProfilesListQueryRepository>()
                    .GetProfileInfoAsync(
                        request.ProfileSubjectId,
                        context.CancellationToken);

            return profileInfo.Adapt<GetProfileInfoResponse>();
        }

        public override async Task<GetStatisticsResponse> GetStatistics(
            Empty request,
            ServerCallContext context)
        {
            IIntegrationServiceStatisticsListQueryRepository.GetStatisticsVO statistics =
                await this.serviceProvider
                    .GetRequiredService<IIntegrationServiceStatisticsListQueryRepository>()
                    .GetStatisticsAsync(context.CancellationToken);

            return statistics.Adapt<GetStatisticsResponse>();
        }

        public override async Task<GetAuthenticationInfoResponse> GetAuthenticationInfo(
            GetAuthenticationInfoRequest request,
            ServerCallContext context)
        {
            IIntegrationServiceProfilesListQueryRepository.GetAuthenticationInfoVO? authenticatedProfile =
                await this.serviceProvider
                    .GetRequiredService<IIntegrationServiceProfilesListQueryRepository>()
                    .GetAuthenticationInfoAsync(
                        request.CertificateThumbprint,
                        request.OperatorIdentifier,
                        context.CancellationToken);

            return new GetAuthenticationInfoResponse
            {
                AuthenticatedProfile = authenticatedProfile?.Adapt<GetAuthenticationInfoResponse.Types.AuthenticatedProfile>()
            };
        }

        public override async Task<GetCodeSenderResponse> GetCodeSender(
            GetCodeSenderRequest request,
            ServerCallContext context)
        {
            IIntegrationServiceCodeMessagesSendQueryRepository.GetCodeSenderVO? codeSender =
                await this.serviceProvider
                    .GetRequiredService<IIntegrationServiceCodeMessagesSendQueryRepository>()
                    .GetCodeSenderAsync(
                        request.CertificateThumbprint,
                        request.OperatorIdentifier,
                        context.CancellationToken);

            return new GetCodeSenderResponse
            {
                Sender = codeSender?.Adapt<GetCodeSenderResponse.Types.Sender>()
            };
        }

        public override async Task<SendMessage1Response> SendMessage1(
            SendMessage1Request request,
            ServerCallContext context)
        {
            SendMessage1CommandResult result =
                await this.serviceProvider
                    .GetRequiredService<IMediator>()
                    .Send(
                        new SendMessage1Command(
                            request.RecipientIdentifier,
                            request.RecipientPhone,
                            request.RecipientEmail,
                            request.RecipientTargetGroupId,
                            request.MessageSubject,
                            request.MessageBody,
                            request.Documents
                                .Select(e => new SendMessage1CommandDocument(
                                    e.FileName,
                                    e.DocumentRegistrationNumber,
                                    e.FileContent.ToByteArray()))
                                .ToArray(),
                            request.ServiceOid,
                            request.SenderProfileId,
                            request.SenderLoginId,
                            request.SendEvent),
                        context.CancellationToken);

            return result.Adapt<SendMessage1Response>();
        }

        public override async Task<SendMessage1WithAccessCodeResponse> SendMessage1WithAccessCode(
            SendMessage1WithAccessCodeRequest request,
            ServerCallContext context)
        {
            SendMessage1WithAccessCodeCommandResult result =
                await this.serviceProvider
                    .GetRequiredService<IMediator>()
                    .Send(
                        new SendMessage1WithAccessCodeCommand(
                            request.RecipientFirstName,
                            request.RecipientMiddleName,
                            request.RecipientLastName,
                            request.RecipientIdentifier,
                            request.RecipientEmail,
                            request.RecipientPhone,
                            request.MessageSubject,
                            request.MessageBody,
                            request.Documents
                                .Select(e => new SendMessage1WithAccessCodeCommandDocument(
                                    e.FileName,
                                    e.DocumentRegistrationNumber,
                                    e.FileContent.ToByteArray()))
                                .ToArray(),
                            request.ServiceOid,
                            request.SenderProfileId,
                            request.SenderLoginId,
                            request.SendEvent),
                        context.CancellationToken);

            return result.Adapt<SendMessage1WithAccessCodeResponse>();
        }

        public override async Task<SendMessageInReplyToResponse> SendMessageInReplyTo(
            SendMessageInReplyToRequest request,
            ServerCallContext context)
        {
            SendMessageInReplyToRequestCommandResult result =
                await this.serviceProvider
                    .GetRequiredService<IMediator>()
                    .Send(
                        new SendMessageInReplyToRequestCommand(
                            request.MessageSubject,
                            request.MessageBody,
                            request.Documents
                                .Select(e => new SendMessageInReplyToRequestCommandDocument(
                                    e.FileName,
                                    e.DocumentRegistrationNumber,
                                    e.FileContent.ToByteArray()))
                                .ToArray(),
                            request.ReplyToMessageId,
                            request.ServiceOid,
                            request.SenderProfileId,
                            request.SenderLoginId,
                            request.SendEvent),
                        context.CancellationToken);

            return result.Adapt<SendMessageInReplyToResponse>();
        }

        public override async Task<SendMessage1OnBehalfOfResponse> SendMessage1OnBehalfOf(
            SendMessage1OnBehalfOfRequest request,
            ServerCallContext context)
        {
            SendMessage1OnBehalfOfCommandResult result =
                await this.serviceProvider
                    .GetRequiredService<IMediator>()
                    .Send(
                        new SendMessage1OnBehalfOfCommand(
                            request.SenderIdentifier,
                            request.SenderPhone,
                            request.SenderEmail,
                            request.SenderFirstName,
                            request.SenderLastName,
                            request.SenderTargetGroupId,
                            request.RecipientIdentifier,
                            request.RecipientTargetGroupId,
                            request.MessageSubject,
                            request.MessageBody,
                            request.Documents
                                .Select(e => new SendMessage1OnBehalfOfCommandDocument(
                                    e.FileName,
                                    e.DocumentRegistrationNumber,
                                    e.FileContent.ToByteArray()))
                                .ToArray(),
                            request.ServiceOid,
                            request.OnBehalfOfProfileId,
                            request.OnBehalfOfLoginId,
                            request.OnBehalfOfOperatorLoginId,
                            request.SendEvent),
                        context.CancellationToken);

            return result.Adapt<SendMessage1OnBehalfOfResponse>();
        }

        public override async Task<SendMessage1OnBehalfOfToIndividualResponse> SendMessage1OnBehalfOfToIndividual(
            SendMessage1OnBehalfOfToIndividualRequest request,
            ServerCallContext context)
        {
            SendMessage1OnBehalfOfToIndividualCommandResult result =
                await this.serviceProvider
                    .GetRequiredService<IMediator>()
                    .Send(
                        new SendMessage1OnBehalfOfToIndividualCommand(
                            request.SenderIdentifier,
                            request.RecipientIdentifier,
                            request.RecipientPhone,
                            request.RecipientEmail,
                            request.RecipientFirstName,
                            request.RecipientLastName,
                            request.MessageSubject,
                            request.MessageBody,
                            request.Documents
                                .Select(e => new SendMessage1OnBehalfOfToIndividualCommandDocument(
                                    e.FileName,
                                    e.DocumentRegistrationNumber,
                                    e.FileContent.ToByteArray()))
                                .ToArray(),
                            request.ServiceOid,
                            request.OnBehalfOfProfileId,
                            request.OnBehalfOfLoginId,
                            request.OnBehalfOfOperatorLoginId,
                            request.SendEvent),
                        context.CancellationToken);

            return result.Adapt<SendMessage1OnBehalfOfToIndividualResponse>();
        }

        public override async Task<SendMessage1OnBehalfOfToLegalEntityResponse> SendMessage1OnBehalfOfToLegalEntity(
            SendMessage1OnBehalfOfToLegalEntityRequest request,
            ServerCallContext context)
        {
            SendMessage1OnBehalfOfToLegalEntityCommandResult result =
                await this.serviceProvider
                    .GetRequiredService<IMediator>()
                    .Send(
                        new SendMessage1OnBehalfOfToLegalEntityCommand(
                            request.SenderIdentifier,
                            request.RecipientIdentifier,
                            request.MessageSubject,
                            request.MessageBody,
                            request.Documents
                                .Select(e => new SendMessage1OnBehalfOfToLegalEntityCommandDocument(
                                    e.FileName,
                                    e.DocumentRegistrationNumber,
                                    e.FileContent.ToByteArray()))
                                .ToArray(),
                            request.ServiceOid,
                            request.OnBehalfOfProfileId,
                            request.OnBehalfOfLoginId,
                            request.OnBehalfOfOperatorLoginId,
                            request.SendEvent),
                        context.CancellationToken);

            return result.Adapt<SendMessage1OnBehalfOfToLegalEntityResponse>();
        }

        public override async Task<GetSentDocumentContentByRegNumResponse> GetSentDocumentContentByRegNum(
            GetSentDocumentContentByRegNumRequest request,
            ServerCallContext context)
        {
            bool checkProfileIsBlobSender =
                await this.serviceProvider
                    .GetRequiredService<IIntegrationServiceMessagesOpenQueryRepository>()
                    .CheckProfileIsBlobSenderByDocumentRegistrationNumberAsync(
                        request.ProfileId,
                        request.DocumentRegistrationNumber,
                        context.CancellationToken);

            if (!checkProfileIsBlobSender)
            {
                throw new RpcException(
                    new Status(
                        StatusCode.InvalidArgument,
                        $"Profile can't read document"));
            }

            IIntegrationServiceMessagesOpenQueryRepository.GetSentDocumentContentByRegNumVO? doc =
                await this.serviceProvider
                    .GetRequiredService<IIntegrationServiceMessagesOpenQueryRepository>()
                    .GetSentDocumentContentByRegNumAsync(
                        request.ProfileId,
                        request.DocumentRegistrationNumber,
                        context.CancellationToken);

            return new GetSentDocumentContentByRegNumResponse
            {
                Blob = doc?.Adapt<GetSentDocumentContentByRegNumResponse.Types.Blob>()
            };
        }

        public override async Task<GetSentDocumentsContentResponse> GetSentDocumentsContent(
            GetSentDocumentsContentRequest request,
            ServerCallContext context)
        {
            IIntegrationServiceMessagesOpenQueryRepository.GetProfileMessageRoleVO? profileMessageRole =
                await this.serviceProvider
                    .GetRequiredService<IIntegrationServiceMessagesOpenQueryRepository>()
                    .GetProfileMessageRoleAsync(
                        request.ProfileId,
                        request.MessageId,
                        context.CancellationToken);

            if (profileMessageRole == null || !profileMessageRole.IsSender)
            {
                throw new RpcException(
                    new Status(
                        StatusCode.InvalidArgument,
                        $"Profile can't read documents"));
            }

            IIntegrationServiceMessagesOpenQueryRepository.GetSentDocumentsContentVO[] docs =
                await this.serviceProvider
                    .GetRequiredService<IIntegrationServiceMessagesOpenQueryRepository>()
                    .GetSentDocumentsContentAsync(
                        request.ProfileId,
                        request.MessageId,
                        context.CancellationToken);

            return new GetSentDocumentsContentResponse
            {
                Blobs =
                {
                    docs.ProjectToType<GetSentDocumentsContentResponse.Types.Blob>()
                }
            };
        }

        public override async Task<GetSentDocumentContentResponse> GetSentDocumentContent(
            GetSentDocumentContentRequest request,
            ServerCallContext context)
        {
            bool checkProfileIsBlobSenderAsync =
                await this.serviceProvider
                    .GetRequiredService<IIntegrationServiceMessagesOpenQueryRepository>()
                    .CheckProfileIsBlobSenderAsync(
                        request.ProfileId,
                        request.BlobId,
                        context.CancellationToken);

            if (!checkProfileIsBlobSenderAsync)
            {
                throw new RpcException(
                    new Status(
                        StatusCode.InvalidArgument,
                        $"Profile can't read document"));
            }

            // TODO: this method's logic is broken as the blob in theory could be attached to more than one message
            IIntegrationServiceMessagesOpenQueryRepository.GetSentDocumentContentVO? doc =
                await this.serviceProvider
                    .GetRequiredService<IIntegrationServiceMessagesOpenQueryRepository>()
                    .GetSentDocumentContentAsync(
                        request.ProfileId,
                        request.BlobId,
                        context.CancellationToken);

            return new GetSentDocumentContentResponse
            {
                Blob = doc?.Adapt<GetSentDocumentContentResponse.Types.Blob>()
            };
        }

        public override async Task<GetSentMessageStatusResponse> GetSentMessageStatus(
            GetSentMessageStatusRequest request,
            ServerCallContext context)
        {
            IIntegrationServiceMessagesOpenQueryRepository.GetProfileMessageRoleVO? profileMessageRole =
                await this.serviceProvider
                    .GetRequiredService<IIntegrationServiceMessagesOpenQueryRepository>()
                    .GetProfileMessageRoleAsync(
                        request.ProfileId,
                        request.MessageId,
                        context.CancellationToken);

            if (profileMessageRole == null
                || (!profileMessageRole.IsSender && !profileMessageRole.IsRecipient))
            {
                throw new RpcException(
                    new Status(
                        StatusCode.InvalidArgument,
                        $"Profile can't read messsage"));
            }

            if (profileMessageRole.IsSender)
            {
                IIntegrationServiceMessagesOpenQueryRepository.GetSentMessageStatusAsSenderVO message =
                     await this.serviceProvider
                        .GetRequiredService<IIntegrationServiceMessagesOpenQueryRepository>()
                        .GetSentMessageStatusAsSenderAsync(
                            request.ProfileId,
                            request.MessageId,
                            context.CancellationToken);

                return new GetSentMessageStatusResponse
                {
                    Message = message.Adapt<GetSentMessageStatusResponse.Types.Message>()
                };
            }
            else
            {
                // this is incorrect logic but it is copied from the original code of the service
                bool firstTimeOpen = await this.serviceProvider
                    .GetRequiredService<IMediator>()
                    .Send(
                        new OpenMessage1Command(
                            request.MessageId,
                            request.ProfileId,
                            request.LoginId,
                            request.OpenEvent),
                        context.CancellationToken);

                IIntegrationServiceMessagesOpenQueryRepository.GetSentMessageStatusAsRecipientVO message =
                     await this.serviceProvider
                        .GetRequiredService<IIntegrationServiceMessagesOpenQueryRepository>()
                        .GetSentMessageStatusAsRecipientAsync(
                            request.ProfileId,
                            request.MessageId,
                            firstTimeOpen,
                            context.CancellationToken);

                return new GetSentMessageStatusResponse
                {
                    Message = message.Adapt<GetSentMessageStatusResponse.Types.Message>()
                };
            }
        }

        public override async Task<GetSentDocumentStatusByRegNumResponse> GetSentDocumentStatusByRegNum(
            GetSentDocumentStatusByRegNumRequest request,
            ServerCallContext context)
        {
            // TODO: this method's logic is broken as document registration number is not unique
            // TODO: and it can't determine a specific message in every case
            bool checkProfileIsMessageSender =
                await this.serviceProvider
                    .GetRequiredService<IIntegrationServiceMessagesOpenQueryRepository>()
                    .CheckProfileIsMessageSenderByDocumentRegistrationNumberAsync(
                        request.ProfileId,
                        request.DocumentRegistrationNumber,
                        context.CancellationToken);

            // in original code of the service a message recipient could use this method to open message
            // this is removed and it is incorrect
            if (!checkProfileIsMessageSender)
            {
                throw new RpcException(
                    new Status(
                        StatusCode.InvalidArgument,
                        $"Profile can't read messsage"));
            }

            IIntegrationServiceMessagesOpenQueryRepository.GetSentDocumentStatusByRegNumVO message =
                await this.serviceProvider
                    .GetRequiredService<IIntegrationServiceMessagesOpenQueryRepository>()
                    .GetSentDocumentStatusByRegNumAsync(
                        request.ProfileId,
                        request.DocumentRegistrationNumber,
                        context.CancellationToken);

            return new GetSentDocumentStatusByRegNumResponse
            {
                Message = message.Adapt<GetSentDocumentStatusByRegNumResponse.Types.Message>()
            };
        }

        public override async Task<GetReceivedMessageContentResponse> GetReceivedMessageContent(
            GetReceivedMessageContentRequest request,
            ServerCallContext context)
        {
            IIntegrationServiceMessagesOpenQueryRepository.GetProfileMessageRoleVO? profileMessageRole =
                await this.serviceProvider
                    .GetRequiredService<IIntegrationServiceMessagesOpenQueryRepository>()
                    .GetProfileMessageRoleAsync(
                        request.ProfileId,
                        request.MessageId,
                        context.CancellationToken);

            if (profileMessageRole == null || !profileMessageRole.IsRecipient)
            {
                throw new RpcException(
                    new Status(
                        StatusCode.InvalidArgument,
                        $"Profile can't open messsage"));
            }

            bool firstTimeOpen = await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new OpenMessage1Command(
                        request.MessageId,
                        request.ProfileId,
                        request.LoginId,
                        request.OpenEvent),
                    context.CancellationToken);

            IIntegrationServiceMessagesOpenQueryRepository.GetReceivedMessageContentVO message =
                 await this.serviceProvider
                    .GetRequiredService<IIntegrationServiceMessagesOpenQueryRepository>()
                    .GetReceivedMessageContentAsync(
                        request.ProfileId,
                        request.MessageId,
                        firstTimeOpen,
                        context.CancellationToken);

            return new GetReceivedMessageContentResponse
            {
                Message = message?.Adapt<GetReceivedMessageContentResponse.Types.Message>()
            };
        }

        public override async Task<GetSentMessageContentResponse> GetSentMessageContent(
            GetSentMessageContentRequest request,
            ServerCallContext context)
        {
            IIntegrationServiceMessagesOpenQueryRepository.GetProfileMessageRoleVO? profileMessageRole =
                await this.serviceProvider
                    .GetRequiredService<IIntegrationServiceMessagesOpenQueryRepository>()
                    .GetProfileMessageRoleAsync(
                        request.ProfileId,
                        request.MessageId,
                        context.CancellationToken);

            if (profileMessageRole == null || !profileMessageRole.IsSender)
            {
                throw new RpcException(
                    new Status(
                        StatusCode.InvalidArgument,
                        $"Profile can't read messsage"));
            }

            IIntegrationServiceMessagesOpenQueryRepository.GetSentMessageContentVO message =
                 await this.serviceProvider
                    .GetRequiredService<IIntegrationServiceMessagesOpenQueryRepository>()
                    .GetSentMessageContentAsync(
                        request.ProfileId,
                        request.MessageId,
                        context.CancellationToken);

            return new GetSentMessageContentResponse
            {
                Message = message?.Adapt<GetSentMessageContentResponse.Types.Message>()
            };
        }
    }
}
