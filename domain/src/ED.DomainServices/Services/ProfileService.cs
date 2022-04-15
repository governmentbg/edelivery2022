using System;
using System.Linq;
using System.Threading.Tasks;
using ED.Domain;
using ED.DomainServices.Profiles;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Mapster;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ED.DomainServices
{
    public class ProfileService : Profiles.Profile.ProfileBase
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IAuthorizationService authorizationService;

        public ProfileService(
            IServiceProvider serviceProvider,
            IAuthorizationService authorizationService)
        {
            this.serviceProvider = serviceProvider;
            this.authorizationService = authorizationService;
        }

        public override async Task<Empty> ArchiveRecipientGroup(
            ArchiveRecipientGroupRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
              .GetRequiredService<IMediator>()
              .Send(
                  new ArchiveProfileRecipientGroupCommand(
                      request.RecipientGroupId,
                      request.LoginId),
                  context.CancellationToken);

            return new Empty();
        }

        public override async Task<CreateRecipientGroupResponse> CreateRecipientGroup(
            CreateRecipientGroupRequest request,
            ServerCallContext context)
        {
            CreateProfileRecipientGroupCommandResult result =
                await this.serviceProvider
                    .GetRequiredService<IMediator>()
                    .Send(
                        new CreateProfileRecipientGroupCommand(
                            request.Name,
                            request.ProfileId,
                            request.LoginId),
                        context.CancellationToken);

            return result.Adapt<CreateRecipientGroupResponse>();
        }

        public override async Task<Empty> DeleteRecipientGroupMember(
            DeleteRecipientGroupMemberRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                   .GetRequiredService<IMediator>()
                   .Send(
                       new DeleteProfileRecipientGroupMemberCommand(
                           request.RecipientGroupId,
                           request.ProfileId,
                           request.LoginId),
                       context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> GrantAccess(
            GrantAccessRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new GrantProfileAccessCommand(
                        request.ProfileId,
                        request.LoginId,
                        request.IsDefault,
                        request.IsEmailNotificationEnabled,
                        request.IsEmailNotificationOnDeliveryEnabled,
                        request.IsSmsNotificationEnabled,
                        request.IsSmsNotificationOnDeliveryEnabled,
                        request.IsViberNotificationEnabled,
                        request.IsViberNotificationOnDeliveryEnabled,
                        request.Details,
                        request.ActionLoginId,
                        request.Ip,
                        request.Permissions.ProjectToType<GrantProfileAccessCommandPermission>().ToArray()),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> RevokeAccess(
            RevokeAccessRequest request,
            ServerCallContext context)
        {
            if (!await this.authorizationService.HasProfileAccessAsync(
                    request.ProfileId,
                    request.ActionLoginId,
                    context.CancellationToken))
            {
                throw new RpcException(
                    new Status(
                        StatusCode.PermissionDenied,
                        $"Login {request.LoginId} does not have access to profile {request.ProfileId}"));
            }

            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new RevokeProfileAccessCommand(
                        request.ProfileId,
                        request.LoginId,
                        request.ActionLoginId,
                        request.Ip),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> UpdateIndividualNames(
            UpdateIndividualNamesRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new UpdateIndividualNamesCommand(
                        request.ProfileId,
                        request.FirstName,
                        request.MiddleName,
                        request.LastName,
                        request.ActionLoginId,
                        request.Ip),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<UpdateResponse> Update(
            UpdateRequest request,
            ServerCallContext context)
        {
            UpdateProfileCommandResult result = await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new UpdateProfileCommand(
                        request.ProfileId,
                        request.Email,
                        request.Phone,
                        request.Residence,
                        request.Sync,
                        request.ActionLoginId,
                        request.Ip),
                    context.CancellationToken);

            return result.Adapt<UpdateResponse>();
        }


        public override async Task<Empty> BringProfileInForce(
            BringProfileInForceRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new BringProfileInForceCommand(
                        request.LoginId,
                        request.IsEmailNotificationEnabled,
                        request.IsEmailNotificationOnDeliveryEnabled,
                        request.IsSmsNotificationEnabled,
                        request.IsSmsNotificationOnDeliveryEnabled,
                        request.IsViberNotificationEnabled,
                        request.IsViberNotificationOnDeliveryEnabled,
                        request.Ip),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> UpdateAccess(
            UpdateAccessRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
               .GetRequiredService<IMediator>()
               .Send(
                   new UpdateProfileAccessCommand(
                       request.ProfileId,
                       request.LoginId,
                       request.Details,
                       request.ActionLoginId,
                       request.Ip,
                       request.Permissions.ProjectToType<GrantProfileAccessCommandPermission>().ToArray()),
                   context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> UpdateRecipientGroup(
            UpdateRecipientGroupRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
               .GetRequiredService<IMediator>()
               .Send(
                   new UpdateProfileRecipientGroupCommand(
                       request.RecipientGroupId,
                       request.Name,
                       request.LoginId),
                   context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> UpdateRecipientGroupMembers(
            UpdateRecipientGroupMembersRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new UpdateProfileRecipientGroupMembersCommand(
                        request.RecipientGroupId,
                        request.ProfileIds.ToArray(),
                        request.LoginId,
                        request.ProfileId),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> UpdateSettings(
            UpdateSettingsRequest request,
            ServerCallContext context)
        {
            if (!await this.authorizationService.HasProfileAccessAsync(
                    request.ProfileId,
                    request.LoginId,
                    context.CancellationToken))
            {
                throw new RpcException(
                    new Status(
                        StatusCode.PermissionDenied,
                        $"Login {request.LoginId} does not have access to profile {request.ProfileId}"));
            }

            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new UpdateProfileSettingsCommand(
                        request.ProfileId,
                        request.LoginId,
                        request.IsEmailNotificationEnabled,
                        request.IsEmailNotificationOnDeliveryEnabled,
                        request.IsSmsNotificationEnabled,
                        request.IsSmsNotificationOnDeliveryEnabled,
                        request.IsViberNotificationEnabled,
                        request.IsViberNotificationOnDeliveryEnabled,
                        request.Email,
                        request.Phone),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<FindIndividualResponse> FindIndividual(
            FindIndividualRequest request,
            ServerCallContext context)
        {
            IProfileAdministerQueryRepository.FindIndividualVO? individual =
                await this.serviceProvider
                    .GetRequiredService<IProfileAdministerQueryRepository>()
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
            IProfileAdministerQueryRepository.FindLegalEntityVO? legalEntity =
                await this.serviceProvider
                    .GetRequiredService<IProfileAdministerQueryRepository>()
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

        public override async Task<FindLoginResponse> FindLogin(
            FindLoginRequest request,
            ServerCallContext context)
        {
            IProfileAdministerQueryRepository.FindLoginVO? login =
                await this.serviceProvider
                    .GetRequiredService<IProfileAdministerQueryRepository>()
                    .FindLoginAsync(
                        request.FirstName,
                        request.LastName,
                        request.Identifier,
                        context.CancellationToken);

            FindLoginResponse response = new();

            if (login != null)
            {
                response.Login = login.Adapt<FindLoginResponse.Types.Login>();
            }

            return response;
        }

        public override async Task<GetBlobsResponse> GetBlobs(
            GetBlobsRequest request,
            ServerCallContext context)
        {
            TableResultVO<IProfileAdministerQueryRepository.GetBlobsVO> blobs =
                await this.serviceProvider
                    .GetRequiredService<IProfileAdministerQueryRepository>()
                    .GetBlobsAsync(request.ProfileId, context.CancellationToken);

            return new GetBlobsResponse
            {
                Length = blobs.Length,
                Result =
                {
                    blobs.Result.ProjectToType<GetBlobsResponse.Types.BlobMessage>()
                }
            };
        }

        public override async Task<GetHistoryResponse> GetHistory(
            GetHistoryRequest request,
            ServerCallContext context)
        {
            TableResultVO<IProfileAdministerQueryRepository.GetHistoryVO> history =
                await this.serviceProvider
                    .GetRequiredService<IProfileAdministerQueryRepository>()
                    .GetHistoryAsync(
                        request.ProfileId,
                        request.Offset,
                        request.Limit,
                        context.CancellationToken);

            return new GetHistoryResponse
            {
                Length = history.Length,
                History =
                {
                    history.Result.ProjectToType<GetHistoryResponse.Types.History>()
                }
            };
        }

        public override async Task<GetIndividualResponse> GetIndividual(
            GetIndividualRequest request,
            ServerCallContext context)
        {
            IProfileAdministerQueryRepository.GetIndividualVO individual =
                await this.serviceProvider
                    .GetRequiredService<IProfileAdministerQueryRepository>()
                    .GetIndividualAsync(
                        request.ProfileId,
                        context.CancellationToken);

            return individual.Adapt<GetIndividualResponse>();
        }

        public override async Task<GetLegalEntityResponse> GetLegalEntity(
            GetLegalEntityRequest request,
            ServerCallContext context)
        {
            IProfileAdministerQueryRepository.GetLegalEntityVO legalEntity =
                await this.serviceProvider
                    .GetRequiredService<IProfileAdministerQueryRepository>()
                    .GetLegalEntityAsync(
                        request.ProfileId,
                        context.CancellationToken);

            return legalEntity.Adapt<GetLegalEntityResponse>();
        }

        public override async Task<GetLoginPermissionsResponse> GetLoginPermissions(
            GetLoginPermissionsRequest request,
            ServerCallContext context)
        {
            IProfileAdministerQueryRepository.GetLoginPermissionsVO loginPermissions =
                await this.serviceProvider
                    .GetRequiredService<IProfileAdministerQueryRepository>()
                    .GetLoginPermissionsAsync(
                        request.ProfileId,
                        request.LoginId,
                        context.CancellationToken);

            return loginPermissions.Adapt<GetLoginPermissionsResponse>();
        }

        public override async Task<GetLoginsResponse> GetLogins(
            GetLoginsRequest request,
            ServerCallContext context)
        {
            if (!await this.authorizationService.HasProfileAccessAsync(
                    request.ProfileId,
                    request.LoginId,
                    context.CancellationToken))
            {
                throw new RpcException(
                    new Status(
                        StatusCode.PermissionDenied,
                        $"Login {request.LoginId} does not have access to profile {request.ProfileId}"));
            }

            IProfileAdministerQueryRepository.GetLoginsVO[] logins =
                await this.serviceProvider
                    .GetRequiredService<IProfileAdministerQueryRepository>()
                    .GetLoginsAsync(
                        request.ProfileId,
                        context.CancellationToken);

            return new GetLoginsResponse
            {
                Logins =
                {
                    logins.ProjectToType<GetLoginsResponse.Types.Login>()
                }
            };
        }

        public override async Task<GetRecipientGroupResponse> GetRecipientGroup(
            GetRecipientGroupRequest request,
            ServerCallContext context)
        {
            IProfileAdministerQueryRepository.GetRecipientGroupVO recipientGroup =
                await this.serviceProvider
                    .GetRequiredService<IProfileAdministerQueryRepository>()
                    .GetRecipientGroupAsync(
                        request.RecipientGroupId,
                        request.ProfileId,
                        context.CancellationToken);

            return recipientGroup.Adapt<GetRecipientGroupResponse>();
        }

        public override async Task<GetRecipientGroupMembersResponse> GetRecipientGroupMembers(
            GetRecipientGroupMembersRequest request,
            ServerCallContext context)
        {
            TableResultVO<IProfileAdministerQueryRepository.GetRecipientGroupMembersVO> recipientGroupMembers =
                await this.serviceProvider
                    .GetRequiredService<IProfileAdministerQueryRepository>()
                    .GetRecipientGroupMembersAsync(
                        request.RecipientGroupId,
                        context.CancellationToken);

            return new GetRecipientGroupMembersResponse
            {
                Length = recipientGroupMembers.Length,
                Members =
                {
                    recipientGroupMembers.Result.ProjectToType<GetRecipientGroupMembersResponse.Types.Member>()
                }
            };
        }

        public override async Task<GetRecipientGroupsResponse> GetRecipientGroups(
            GetRecipientGroupsRequest request,
            ServerCallContext context)
        {
            TableResultVO<IProfileAdministerQueryRepository.GetRecipientGroupsVO> recipientGroups =
                await this.serviceProvider
                    .GetRequiredService<IProfileAdministerQueryRepository>()
                    .GetRecipientGroupsAsync(
                        request.ProfileId,
                        request.Offset,
                        request.Limit,
                        context.CancellationToken);

            return new GetRecipientGroupsResponse
            {
                Length = recipientGroups.Length,
                RecipientGroups =
                {
                    recipientGroups.Result.ProjectToType<GetRecipientGroupsResponse.Types.RecipientGroup>()
                }
            };
        }

        public override async Task<GetRecipientGroupsCountResponse> GetRecipientGroupsCount(
            GetRecipientGroupsCountRequest request,
            ServerCallContext context)
        {
            IProfileAdministerQueryRepository.GetRecipientGroupsCountVO recipientGroups =
                await this.serviceProvider
                    .GetRequiredService<IProfileAdministerQueryRepository>()
                    .GetRecipientGroupsCountAsync(
                        request.ProfileId,
                        context.CancellationToken);

            return recipientGroups.Adapt<GetRecipientGroupsCountResponse>();
        }

        public override async Task<GetTargetGroupsResponse> GetTargetGroups(
            GetTargetGroupsRequest request,
            ServerCallContext context)
        {
            TableResultVO<IProfileAdministerQueryRepository.GetTargetGroupsVO> targetGroups =
                await this.serviceProvider
                    .GetRequiredService<IProfileAdministerQueryRepository>()
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

        public override async Task<GetTemplatesResponse> GetTemplates(
            GetTemplatesRequest request,
            ServerCallContext context)
        {
            TableResultVO<IProfileAdministerQueryRepository.GetAllowedTemplatesVO> templates =
                await this.serviceProvider
                    .GetRequiredService<IProfileAdministerQueryRepository>()
                    .GetAllowedTemplatesAsync(
                        request.ProfileId,
                        context.CancellationToken);

            return new GetTemplatesResponse
            {
                Length = templates.Length,
                Result =
                {
                    templates.Result.ProjectToType<GetTemplatesResponse.Types.TemplateMessage>()
                }
            };
        }

        public override async Task<GetSettingsResponse> GetSettings(
           GetSettingsRequest request,
           ServerCallContext context)
        {
            if (!await this.authorizationService.HasProfileAccessAsync(
                    request.ProfileId,
                    request.LoginId,
                    context.CancellationToken))
            {
                throw new RpcException(
                    new Status(
                        StatusCode.PermissionDenied,
                        $"Login {request.LoginId} does not have access to profile {request.ProfileId}"));
            }

            IProfileAdministerQueryRepository.GetSettingsVO settings =
                await this.serviceProvider
                    .GetRequiredService<IProfileAdministerQueryRepository>()
                    .GetSettingsAsync(
                        request.ProfileId,
                        request.LoginId,
                        context.CancellationToken);

            return settings.Adapt<GetSettingsResponse>();
        }

        public override async Task<GetPassiveProfileDataResponse> GetPassiveProfileData(
            GetPassiveProfileDataRequest request,
            ServerCallContext context)
        {
            IProfileAdministerQueryRepository.GetPassiveProfileDataVO profileData =
                await this.serviceProvider
                    .GetRequiredService<IProfileAdministerQueryRepository>()
                    .GetPassiveProfileDataAsync(
                        request.LoginId,
                        context.CancellationToken);

            return profileData.Adapt<GetPassiveProfileDataResponse>();
        }

        public override async Task<Empty> CreateAccessProfilesHistory(
            CreateAccessProfilesHistoryRequest request,
            ServerCallContext context)
        {
            if (!await this.authorizationService.HasProfileAccessAsync(
                    request.ProfileId,
                    request.LoginId,
                    context.CancellationToken))
            {
                throw new RpcException(
                    new Status(
                        StatusCode.PermissionDenied,
                        $"Login {request.LoginId} does not have access to profile {request.ProfileId}"));
            }

            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new CreateAccessProfilesHistoryCommand(
                        request.ProfileId,
                        request.LoginId,
                        request.Ip),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<GetLoginProfilesResponse> GetLoginProfiles(
            GetLoginProfilesRequest request,
            ServerCallContext context)
        {
            IProfileListQueryRepository.GetLoginProfilesVO[] loginProfiles =
                await this.serviceProvider
                    .GetRequiredService<IProfileListQueryRepository>()
                    .GetLoginProfilesAsync(
                        request.LoginId,
                        context.CancellationToken);

            return new GetLoginProfilesResponse
            {
                LoginProfiles =
                {
                    loginProfiles.ProjectToType<GetLoginProfilesResponse.Types.LoginProfile>()
                }
            };
        }

        public override async Task<GetStatisticsResponse> GetStatistics(
            Empty request,
            ServerCallContext context)
        {
            IProfileListQueryRepository.GetStatisticsVO statistics =
                await this.serviceProvider
                    .GetRequiredService<IProfileListQueryRepository>()
                    .GetStatisticsAsync(context.CancellationToken);

            return statistics.Adapt<GetStatisticsResponse>();
        }

        public override async Task<GetTargetGroupProfilesResponse> GetTargetGroupProfiles(
            GetTargetGroupProfilesRequest request,
            ServerCallContext context)
        {
            TableResultVO<IProfileListQueryRepository.GetTargetGroupProfilesVO> targetGroupProfiles =
                await this.serviceProvider
                    .GetRequiredService<IProfileListQueryRepository>()
                    .GetTargetGroupProfilesAsync(
                        request.TargetGroupId,
                        request.Term,
                        request.Offset,
                        request.Limit,
                        context.CancellationToken);

            return new GetTargetGroupProfilesResponse
            {
                Length = targetGroupProfiles.Length,
                Result =
                {
                    targetGroupProfiles.Result.ProjectToType<GetTargetGroupProfilesResponse.Types.Profile>()
                }
            };
        }

        public override async Task<CreateOrUpdateIndividualResponse> CreateOrUpdateIndividual(
            CreateOrUpdateIndividualRequest request,
            ServerCallContext context)
        {
            (int profileId, Guid profileGuid, string profileName) =
                await this.serviceProvider
                    .GetRequiredService<IMediator>()
                    .Send(
                        new CreateOrUpdateIndividualCommand(
                            request.FistName,
                            request.MiddleName,
                            request.LastName,
                            request.Identifier,
                            request.Phone,
                            request.Email,
                            request.Residence,
                            request.IsPassive,
                            request.IsEmailNotificationEnabled,
                            request.IsSmsNotificationEnabled,
                            request.IsViberNotificationEnabled,
                            request.ActionLoginId,
                            request.Ip),
                        context.CancellationToken);

            return new CreateOrUpdateIndividualResponse
            {
                ProfileId = profileId,
                ProfileGuid = profileGuid.ToString(),
                ProfileName = profileName
            };
        }

        public override async Task<CreateRegisterRequestResponse> CreateRegisterRequest(
            CreateRegisterRequestRequest request,
            ServerCallContext context)
        {
            CreateRegisterRequestCommandResult registration =
                await this.serviceProvider
                    .GetRequiredService<IMediator>()
                    .Send(
                        new CreateRegisterRequestCommand(
                            request.RegistrationEmail,
                            request.RegistrationPhone,
                            request.RegistrationIsEmailNotificationEnabled,
                            request.RegistrationIsSmsNotificationEnabled,
                            request.RegistrationIsViberNotificationEnabled,
                            request.Name,
                            request.Identifier,
                            request.Phone,
                            request.Email,
                            request.Residence,
                            request.City,
                            request.State,
                            request.Country,
                            request.TargetGroupId,
                            request.BlobId,
                            request.LoginId),
                        context.CancellationToken);

            return registration.Adapt<CreateRegisterRequestResponse>();
        }

        public override async Task<ParseRegistrationDocumentResponse> ParseRegistrationDocument(
            ParseRegistrationDocumentRequest request,
            ServerCallContext context)
        {
            IProfileRegisterQueryRepository.ParseRegistrationDocumentVO document =
                await this.serviceProvider
                    .GetRequiredService<IProfileRegisterQueryRepository>()
                    .ParseRegistrationDocumentAsync(
                        request.BlobId,
                        context.CancellationToken);

            return document.Adapt<ParseRegistrationDocumentResponse>();
        }

        public override async Task<GetRegisteredIndividualResponse> GetRegisteredIndividual(
            GetRegisteredIndividualRequest request,
            ServerCallContext context)
        {
            IProfileRegisterQueryRepository.GetRegisteredIndividualVO? profile =
               await this.serviceProvider
                   .GetRequiredService<IProfileRegisterQueryRepository>()
                   .GetRegisteredIndividualAsync(
                        request.Identifier,
                        context.CancellationToken);

            return new GetRegisteredIndividualResponse
            {
                Profile = profile?.Adapt<GetRegisteredIndividualResponse.Types.Profile>()
            };
        }

        public override async Task<GetProfileResponse> GetProfile(
            GetProfileRequest request,
            ServerCallContext context)
        {
            IProfileSeosQueryRepository.GetProfileVO profile =
               await this.serviceProvider
                   .GetRequiredService<IProfileSeosQueryRepository>()
                   .GetProfileAsync(
                        request.ProfileId,
                        context.CancellationToken);

            return profile.Adapt<GetProfileResponse>();
        }

        public override async Task<CheckIndividualUniquenessResponse> CheckIndividualUniqueness(
            CheckIndividualUniquenessRequest request,
            ServerCallContext context)
        {
            IProfileRegisterQueryRepository.CheckIndividualUniquenessVO unique =
               await this.serviceProvider
                   .GetRequiredService<IProfileRegisterQueryRepository>()
                   .CheckIndividualUniquenessAsync(
                        request.Identifier,
                        request.Email,
                        context.CancellationToken);

            return unique.Adapt<CheckIndividualUniquenessResponse>();
        }
    }
}
