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
    public class EsbService : Esb.Esb.EsbBase
    {
        private readonly IServiceProvider serviceProvider;

        public EsbService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public override async Task<InboxResponse> Inbox(
            BoxRequest request,
            ServerCallContext context)
        {
            TableResultVO<IEsbMessagesListQueryRepository.GetInboxVO> inbox =
                await this.serviceProvider
                    .GetRequiredService<IEsbMessagesListQueryRepository>()
                    .GetInboxAsync(
                        request.ProfileId,
                        request.From?.ToLocalDateTime(),
                        request.To?.ToLocalDateTime(),
                        request.TemplateId,
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

        public override async Task<OutboxResponse> Outbox(
            BoxRequest request,
            ServerCallContext context)
        {
            TableResultVO<IEsbMessagesListQueryRepository.GetOutboxVO> outbox =
                await this.serviceProvider
                    .GetRequiredService<IEsbMessagesListQueryRepository>()
                    .GetOutboxAsync(
                        request.ProfileId,
                        request.From?.ToLocalDateTime(),
                        request.To?.ToLocalDateTime(),
                        request.TemplateId,
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

        public override async Task<GetMessagesStatisticsResponse> GetMessageReceivedStatistics(
            GetMessagesStatisticsRequest request,
            ServerCallContext context)
        {
            CreateOrGetStatisticsCommandResult[] statistics = await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new CreateOrGetStatisticsCommand(
                        request.MontDate?.ToLocalDateTime(),
                        CreateOrGetStatisticsCommandType.Received),
                    context.CancellationToken);

            return new GetMessagesStatisticsResponse
            {
                Result =
                {
                    statistics.ProjectToType<GetMessagesStatisticsResponse.Types.Statistics>()
                }
            };
        }

        public override async Task<GetMessagesStatisticsResponse> GetMessageSentStatistics(
            GetMessagesStatisticsRequest request,
            ServerCallContext context)
        {
            CreateOrGetStatisticsCommandResult[] statistics = await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new CreateOrGetStatisticsCommand(
                        request.MontDate?.ToLocalDateTime(),
                        CreateOrGetStatisticsCommandType.Sent),
                    context.CancellationToken);

            return new GetMessagesStatisticsResponse
            {
                Result =
                {
                    statistics.ProjectToType<GetMessagesStatisticsResponse.Types.Statistics>()
                }
            };
        }

        public override async Task<GetSentMessageCountResponse> GetSentMessageCount(
            GetSentMessageCountRequest request,
            ServerCallContext context)
        {
            int value = await this.serviceProvider
                .GetRequiredService<IEsbStatisticsListQueryRepository>()
                .GetSentMessagesCountAsync(
                    request.FromDate.ToLocalDateTime(),
                    request.ToDate.ToLocalDateTime(),
                    context.CancellationToken);

            return new GetSentMessageCountResponse
            {
                Value = value
            };
        }

        public override async Task<GetReceivedMessageCountResponse> GetReceivedMessageCount(
            GetReceivedMessageCountRequest request,
            ServerCallContext context)
        {
            int value = await this.serviceProvider
                .GetRequiredService<IEsbStatisticsListQueryRepository>()
                .GetReceivedMessagesCountAsync(
                    request.FromDate.ToLocalDateTime(),
                    request.ToDate.ToLocalDateTime(),
                    context.CancellationToken);

            return new GetReceivedMessageCountResponse
            {
                Value = value
            };
        }

        public override async Task<CheckProfileTargetGroupAccessResponse> CheckProfileTargetGroupAccess(
            CheckProfileTargetGroupAccessRequest request,
            ServerCallContext context)
        {
            bool hasAccess =
                await this.serviceProvider
                    .GetRequiredService<IEsbProfilesListQueryRepository>()
                    .CheckProfileTargetGroupAccessAsync(
                        request.ProfileId,
                        request.TargetGroupId,
                        context.CancellationToken);

            return new CheckProfileTargetGroupAccessResponse
            {
                HasAccess = hasAccess
            };
        }

        public override async Task<CheckProfileOnBehalfOfAccessResponse> CheckProfileOnBehalfOfAccess(
            CheckProfileOnBehalfOfAccessRequest request,
            ServerCallContext context)
        {
            bool hasAccess =
                await this.serviceProvider
                    .GetRequiredService<IEsbProfilesListQueryRepository>()
                    .CheckProfileOnBehalfOfAccessAsync(
                        request.ProfileId,
                        context.CancellationToken);

            return new CheckProfileOnBehalfOfAccessResponse
            {
                HasAccess = hasAccess
            };
        }

        public override async Task<CheckExistingIndividualResponse> CheckExistingIndividual(
            CheckExistingIndividualRequest request,
            ServerCallContext context)
        {
            bool isExisting =
                await this.serviceProvider
                    .GetRequiredService<IEsbProfilesRegisterQueryRepository>()
                    .CheckExistingIndividualAsync(
                        request.Identifier,
                        context.CancellationToken);

            return new CheckExistingIndividualResponse
            {
                IsExisting = isExisting
            };
        }

        public override async Task<GetTargetGroupProfilesResponse> GetTargetGroupProfiles(
            GetTargetGroupProfilesRequest request,
            ServerCallContext context)
        {
            TableResultVO<IEsbProfilesListQueryRepository.GetTargetGroupProfilesVO> profiles =
                await this.serviceProvider
                    .GetRequiredService<IEsbProfilesListQueryRepository>()
                    .GetTargetGroupProfilesAsync(
                        request.TargetGroupId,
                        request.Offset,
                        request.Limit,
                        context.CancellationToken);

            return new GetTargetGroupProfilesResponse
            {
                Length = profiles.Length,
                Result =
                {
                    profiles.Result.ProjectToType<GetTargetGroupProfilesResponse.Types.Profile>()
                }
            };
        }

        public override async Task<SearchTargetGroupProfilesResponse> SearchTargetGroupProfiles(
            SearchTargetGroupProfilesRequest request,
            ServerCallContext context)
        {
            IEsbProfilesListQueryRepository.SearchGetTargetGroupProfilesVO? profile =
                await this.serviceProvider
                    .GetRequiredService<IEsbProfilesListQueryRepository>()
                    .SearchGetTargetGroupProfilesAsync(
                        request.Identifier,
                        request.TemplateId,
                        request.TargetGroupId,
                        context.CancellationToken);

            return new SearchTargetGroupProfilesResponse
            {
                Result = profile?.Adapt<SearchTargetGroupProfilesResponse.Types.Profile>()
            };
        }

        public override async Task<GetTargetGroupsResponse> GetTargetGroups(
            GetTargetGroupsRequest request,
            ServerCallContext context)
        {
            IEsbTargetGroupsListQueryRepository.GetTargetGroupsVO[] targetGroups =
                await this.serviceProvider
                    .GetRequiredService<IEsbTargetGroupsListQueryRepository>()
                    .GetTargetGroupsAsync(
                        request.ProfileId,
                        context.CancellationToken);

            return new GetTargetGroupsResponse
            {
                Result =
                {
                    targetGroups.ProjectToType<GetTargetGroupsResponse.Types.TargetGroup>()
                }
            };
        }

        public override async Task<CreatePassiveIndividualResponse> CreatePassiveIndividual(
            CreatePassiveIndividualRequest request,
            ServerCallContext context)
        {
            int profileId = await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new EsbCreatePassiveIndividualCommand(
                        request.Identifier,
                        request.FirstName,
                        request.MiddleName,
                        request.LastName,
                        request.Phone,
                        request.Email,
                        request.Address.Adapt<EsbCreatePassiveIndividualCommandAddress>(),
                        request.ActionLoginId,
                        request.Ip),
                    context.CancellationToken);

            return new CreatePassiveIndividualResponse
            {
                ProfileId = profileId
            };
        }

        public override async Task<GetTemplatesResponse> GetTemplates(
            GetTemplatesRequest request,
            ServerCallContext context)
        {
            IEsbTemplatesListQueryRepository.GetTemplatesVO[] templates =
                await this.serviceProvider
                    .GetRequiredService<IEsbTemplatesListQueryRepository>()
                    .GetTemplatesAsync(
                        request.ProfileId,
                        context.CancellationToken);

            return new GetTemplatesResponse
            {
                Result =
                {
                    templates.ProjectToType<GetTemplatesResponse.Types.Temaplate>()
                }
            };
        }

        public override async Task<CheckProfileTemplateAccessResponse> CheckProfileTemplateAccess(
            CheckProfileTemplateAccessRequest request,
            ServerCallContext context)
        {
            bool hasAccess =
                await this.serviceProvider
                    .GetRequiredService<IEsbTemplatesListQueryRepository>()
                    .CheckProfileTemplateAccessAsync(
                        request.ProfileId,
                        request.TemplateId,
                        context.CancellationToken);

            return new CheckProfileTemplateAccessResponse
            {
                HasAccess = hasAccess
            };
        }

        public override async Task<GetTemplateResponse> GetTemplate(
            GetTemplateRequest request,
            ServerCallContext context)
        {
            IEsbTemplatesListQueryRepository.GetTemplateVO template =
                await this.serviceProvider
                    .GetRequiredService<IEsbTemplatesListQueryRepository>()
                    .GetTemplateAsync(
                        request.TemplateId,
                        context.CancellationToken);

            return new GetTemplateResponse
            {
                Result = template?.Adapt<GetTemplateResponse.Types.Template>()
            };
        }

        public override async Task<ViewMessageResponse> ViewMessage(
            ViewMessageRequest request,
            ServerCallContext context)
        {
            IEsbMessagesOpenHORepository.GetAsSenderVO message =
                await this.serviceProvider
                    .GetRequiredService<IEsbMessagesOpenHORepository>()
                    .GetAsSenderAsync(
                        request.MessageId,
                        context.CancellationToken);

            return new ViewMessageResponse
            {
                Message = message?.Adapt<ViewMessageResponse.Types.Message>()
            };
        }

        public override async Task<GetMessageResponse> GetMessage(
            GetMessageRequest request,
            ServerCallContext context)
        {
            IEsbMessagesOpenHORepository.GetAsRecipientVO message =
                await this.serviceProvider
                    .GetRequiredService<IEsbMessagesOpenHORepository>()
                    .GetAsRecipientAsync(
                        request.MessageId,
                        request.ProfileId,
                        context.CancellationToken);

            return new GetMessageResponse
            {
                Message = message?.Adapt<GetMessageResponse.Types.Message>()
            };
        }

        public override async Task<OpenMessageResponse> OpenMessage(
            OpenMessageRequest request,
            ServerCallContext context)
        {
            int? templateId =
                await this.serviceProvider
                    .GetRequiredService<IEsbMessagesOpenQueryRepository>()
                    .GetMessageTemplateAsync(
                        request.MessageId,
                        context.CancellationToken);

            if (templateId == Template.TicketTemplate)
            {
                EsbOpenTicketCommandResult? result =
                    await this.serviceProvider
                        .GetRequiredService<IMediator>()
                        .Send(
                            new EsbOpenTicketCommand(
                                request.MessageId,
                                request.ProfileId,
                                request.LoginId),
                            context.CancellationToken);

                return new OpenMessageResponse
                {
                    Result = result?.Adapt<OpenMessageResponse.Types.MessageInfo>()
                };
            }
            else
            {
                EsbOpenMessageCommandResult? result =
                    await this.serviceProvider
                        .GetRequiredService<IMediator>()
                        .Send(
                            new EsbOpenMessageCommand(
                                request.MessageId,
                                request.ProfileId,
                                request.LoginId),
                            context.CancellationToken);

                return new OpenMessageResponse
                {
                    Result = result?.Adapt<OpenMessageResponse.Types.MessageInfo>()
                };
            }
        }

        public override async Task<GetEsbUserResponse> GetEsbUser(
            GetEsbUserRequest request,
            ServerCallContext context)
        {
            IEsbProfilesAuthenticateQueryRepository.GetEsbUserVO? esbUser =
                await this.serviceProvider
                    .GetRequiredService<IEsbProfilesAuthenticateQueryRepository>()
                    .GetEsbUserAsync(
                        request.OId,
                        request.ClientId,
                        request.OperatorIdentifier,
                        request.RepresentedProfileIdentifier,
                        context.CancellationToken);

            return new GetEsbUserResponse
            {
                Result = esbUser?.Adapt<GetEsbUserResponse.Types.EsbUser>()
            };
        }

        public override async Task<CheckMessageRecipientsResponse> CheckMessageRecipients(
            CheckMessageRecipientsRequest request,
            ServerCallContext context)
        {
            bool ok =
                await this.serviceProvider
                    .GetRequiredService<IEsbMessagesSendQueryRepository>()
                    .CheckMessageRecipientsAsync(
                        request.RecipientProfileIds.ToArray(),
                        request.ProfileId,
                        context.CancellationToken);

            return new CheckMessageRecipientsResponse
            {
                IsOk = ok
            };
        }

        public override async Task<SendMessageResponse> SendMessage(
            SendMessageRequest request,
            ServerCallContext context)
        {
            int messageId = await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new EsbSendMessageCommand(
                        request.RecipientProfileIds.Distinct().ToArray(),
                        request.SenderProfileId,
                        request.SenderLoginId,
                        request.SenderViaLoginId,
                        request.TemplateId,
                        request.Subject,
                        request.Rnu,
                        request.Body,
                        request.MetaFields,
                        request.SenderLoginId,
                        request.BlobIds.ToArray(),
                        request.ForwardedMessageId),
                    context.CancellationToken);

            return new SendMessageResponse
            {
                MessageId = messageId
            };
        }

        public override async Task<GetForwardedMessageOriginalRecipientProfileResponse> GetForwardedMessageOriginalRecipientProfile(
            GetForwardedMessageOriginalRecipientProfileRequest request,
            ServerCallContext context)
        {
            int recipientProfileId =
                await this.serviceProvider
                    .GetRequiredService<IEsbMessagesOpenQueryRepository>()
                    .GetForwardedMessageOriginalRecipientProfile(
                        request.MessageId,
                        request.ForwardedMessageId,
                        context.CancellationToken);

            return new GetForwardedMessageOriginalRecipientProfileResponse
            {
                RecipientProfileId = recipientProfileId
            };
        }

        public override async Task<GetBlobsInfoResponse> GetBlobsInfo(
            GetBlobsInfoRequest request,
            ServerCallContext context)
        {
            IEsbMessagesSendQueryRepository.GetBlobsInfoVO[] blobsInfo =
                await this.serviceProvider
                    .GetRequiredService<IEsbMessagesSendQueryRepository>()
                    .GetBlobsInfoAsync(
                        request.BlobIds.ToArray(),
                        context.CancellationToken);

            return new GetBlobsInfoResponse
            {
                Result =
                {
                    blobsInfo.ProjectToType<GetBlobsInfoResponse.Types.Blob>()
                }
            };
        }

        public override async Task<GetStorageBlobsResponse> GetStorageBlobs(
            GetStorageBlobsRequest request,
            ServerCallContext context)
        {
            TableResultVO<IEsbBlobsListQueryRepository.GetStorageBlobsVO> blobs =
              await this.serviceProvider
                  .GetRequiredService<IEsbBlobsListQueryRepository>()
                  .GetStorageBlobsAsync(
                      request.ProfileId,
                      request.Offset,
                      request.Limit,
                      context.CancellationToken);

            return new GetStorageBlobsResponse
            {
                Length = blobs.Length,
                Result =
                {
                    blobs.Result.ProjectToType<GetStorageBlobsResponse.Types.Blob>()
                }
            };
        }

        public override async Task<GetStorageBlobInfoResponse> GetStorageBlobInfo(
            GetStorageBlobInfoRequest request,
            ServerCallContext context)
        {
            IEsbBlobsListQueryRepository.GetStorageBlobInfoVO? blob =
              await this.serviceProvider
                  .GetRequiredService<IEsbBlobsListQueryRepository>()
                  .GetStorageBlobInfoAsync(
                      request.ProfileId,
                      request.BlobId,
                      context.CancellationToken);

            return new GetStorageBlobInfoResponse
            {
                Result = blob?.Adapt<GetStorageBlobInfoResponse.Types.Blob>()
            };
        }

        public override async Task<Empty> DeteleStorageBlob(
            DeteleStorageBlobRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new EsbRemoveStorageBlobCommand(
                        request.ProfileId,
                        request.BlobId),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<CheckStorageBlobResponse> CheckStorageBlob(
            CheckStorageBlobRequest request,
            ServerCallContext context)
        {
            IEsbBlobsListQueryRepository.CheckStorageBlobVO checkStorageBlob =
              await this.serviceProvider
                  .GetRequiredService<IEsbBlobsListQueryRepository>()
                  .CheckStorageBlobAsync(
                      request.ProfileId,
                      request.BlobId,
                      context.CancellationToken);

            return checkStorageBlob.Adapt<CheckStorageBlobResponse>();
        }

        public override async Task<GetRegisteredProfilesResponse> GetRegisteredProfiles(
            GetRegisteredProfilesRequest request,
            ServerCallContext context)
        {
            TableResultVO<IEsbProfilesRegisterQueryRepository.GetRegisteredProfilesVO> profiles =
                await this.serviceProvider
                    .GetRequiredService<IEsbProfilesRegisterQueryRepository>()
                    .GetRegisteredProfilesAsync(
                        request.Identifier,
                        request.Offset,
                        request.Limit,
                        context.CancellationToken);

            return new GetRegisteredProfilesResponse
            {
                Length = profiles.Length,
                Result =
                {
                    profiles.Result.ProjectToType<GetRegisteredProfilesResponse.Types.Profile>()
                }
            };
        }

        public override async Task<CreateOrUpdateIndividualResponse> CreateOrUpdateIndividual(
            CreateOrUpdateIndividualRequest request,
            ServerCallContext context)
        {
            int profileId = await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new EsbCreateOrUpdateIndividualCommand(
                        request.Identifier,
                        request.FirstName,
                        request.MiddleName,
                        request.LastName,
                        request.Phone,
                        request.Email,
                        request.Address.Adapt<EsbCreateOrUpdateIndividualCommandAddress>(),
                        request.IsEmailNotificationEnabled,
                        request.IsFullFeatured,
                        request.ActionLoginId,
                        request.Ip),
                    context.CancellationToken);

            return new CreateOrUpdateIndividualResponse
            {
                ProfileId = profileId
            };
        }

        public override async Task<GetCountriesResponse> GetCountries(
            GetCountriesRequest request,
            ServerCallContext context)
        {
            TableResultVO<IEsbCountriesListQueryRepository.GetCountriesVO> countries =
               await this.serviceProvider
                   .GetRequiredService<IEsbCountriesListQueryRepository>()
                   .GetCountriesAsync(
                       request.Offset,
                       request.Limit,
                       context.CancellationToken);

            return new GetCountriesResponse
            {
                Length = countries.Length,
                Result =
                {
                    countries.Result.ProjectToType<GetCountriesResponse.Types.Country>()
                }
            };
        }

        public override async Task<CheckCountryIsoResponse> CheckCountryIso(
            CheckCountryIsoRequest request,
            ServerCallContext context)
        {
            bool isValid =
                await this.serviceProvider
                    .GetRequiredService<IEsbProfilesRegisterQueryRepository>()
                    .CheckCountryIsoAsync(
                        request.Iso,
                        context.CancellationToken);

            return new CheckCountryIsoResponse
            {
                IsValid = isValid
            };
        }

        public override async Task<CheckTargetGroupIdResponse> CheckTargetGroupId(
            CheckTargetGroupIdRequest request,
            ServerCallContext context)
        {
            bool isValid =
                await this.serviceProvider
                    .GetRequiredService<IEsbProfilesRegisterQueryRepository>()
                    .CheckTargetGroupIdAsync(
                        request.TargetGroupId,
                        context.CancellationToken);

            return new CheckTargetGroupIdResponse
            {
                IsValid = isValid
            };
        }

        public override async Task<CheckExistingLegalEntityResponse> CheckExistingLegalEntity(
            CheckExistingLegalEntityRequest request,
            ServerCallContext context)
        {
            bool isExisting =
                await this.serviceProvider
                    .GetRequiredService<IEsbProfilesRegisterQueryRepository>()
                    .CheckExistingLegalEntityAsync(
                        request.Identifier,
                        request.TargetGroupId,
                        context.CancellationToken);

            return new CheckExistingLegalEntityResponse
            {
                IsExisting = isExisting
            };
        }

        public override async Task<CreateLegalEntityResponse> CreateLegalEntity(
            CreateLegalEntityRequest request,
            ServerCallContext context)
        {
            int profileId = await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new EsbCreateLegalEntityCommand(
                        request.Identifier,
                        request.Name,
                        request.Email,
                        request.Phone,
                        request.Address.Adapt<EsbCreateLegalEntityCommandAddress>(),
                        request.TargetGroupId,
                        request.OwnersData.ProjectToType<EsbCreateLegalEntityCommandOwnerData>().ToArray(),
                        request.ActionLoginId,
                        request.Ip),
                    context.CancellationToken);

            return new CreateLegalEntityResponse
            {
                ProfileId = profileId
            };
        }

        public override async Task<CheckAllLoginsExistResponse> CheckAllLoginsExist(
            CheckAllLoginsExistRequest request,
            ServerCallContext context)
        {
            bool isValid =
                await this.serviceProvider
                    .GetRequiredService<IEsbProfilesRegisterQueryRepository>()
                    .CheckAllLoginsExistAsync(
                        request.Identifiers.ToArray(),
                        context.CancellationToken);

            return new CheckAllLoginsExistResponse
            {
                IsValid = isValid
            };
        }

        public override async Task<GetProfileResponse> GetProfile(
            GetProfileRequest request,
            ServerCallContext context)
        {
            IEsbProfilesListQueryRepository.GetProfileVO? profile =
                await this.serviceProvider
                    .GetRequiredService<IEsbProfilesListQueryRepository>()
                    .GetProfileAsync(
                        request.ProfileId,
                        context.CancellationToken);

            return new GetProfileResponse
            {
                Result = profile?.Adapt<GetProfileResponse.Types.Profile>()
            };
        }
    }
}
