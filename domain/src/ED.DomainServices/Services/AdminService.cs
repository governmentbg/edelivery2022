using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ED.Domain;
using ED.DomainServices.Admin;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Mapster;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ED.DomainServices
{
    public class AdminService : Admin.Admin.AdminBase
    {
        private readonly IServiceProvider serviceProvider;
        public AdminService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public override async Task<GetProfileListResponse> GetProfileList(
            GetProfileListRequest request,
            ServerCallContext context)
        {
            TableResultVO<IAdminProfileListQueryRepository.GetProfilesVO> profiles =
                await this.serviceProvider
                    .GetRequiredService<IAdminProfileListQueryRepository>()
                    .GetProfilesAsync(
                        request.AdminUserId,
                        request.Identifier,
                        request.NameEmailPhone,
                        request.Offset,
                        request.Limit,
                        context.CancellationToken);

            return new GetProfileListResponse
            {
                Length = profiles.Length,
                Result =
                {
                    profiles.Result.ProjectToType<GetProfileListResponse.Types.Profile>()
                }
            };
        }

        public override async Task<GetProfileInfoResponse> GetProfileInfo(
            GetProfileInfoRequest request,
            ServerCallContext context)
        {
            IAdminProfilesCreateEditViewQueryRepository.GetProfileInfoVO profileInfo =
                await this.serviceProvider
                    .GetRequiredService<IAdminProfilesCreateEditViewQueryRepository>()
                    .GetProfileInfoAsync(
                        request.AdminUserId,
                        request.ProfileId,
                        context.CancellationToken);

            return profileInfo.Adapt<GetProfileInfoResponse>();
        }

        public override async Task<GetProfileDataResponse> GetProfileData(
            GetProfileDataRequest request,
            ServerCallContext context)
        {
            IAdminProfilesCreateEditViewQueryRepository.GetProfileDataVO profileData =
                await this.serviceProvider
                    .GetRequiredService<IAdminProfilesCreateEditViewQueryRepository>()
                    .GetProfileDataAsync(
                        request.AdminUserId,
                        request.ProfileId,
                        context.CancellationToken);

            return profileData.Adapt<GetProfileDataResponse>();
        }

        public override async Task<GetProfileAccessAllowedTemplatesResponse>
            GetProfileAccessAllowedTemplates(
                GetProfileAccessAllowedTemplatesRequest request,
                ServerCallContext context)
        {
            IAdminProfilesCreateEditViewQueryRepository.GetAllowedTemplatesVO[] templates =
                await this.serviceProvider
                    .GetRequiredService<IAdminProfilesCreateEditViewQueryRepository>()
                    .GetAllowedTemplatesAsync(
                        request.ProfileId,
                        context.CancellationToken);

            return new GetProfileAccessAllowedTemplatesResponse
            {
                Templates =
                {
                    templates.ProjectToType<GetProfileAccessAllowedTemplatesResponse.Types.AllowedTemplate>()
                }
            };
        }

        public override async Task<GetProfileAccessIndividualByIdentifierResponse>
            GetProfileAccessIndividualByIdentifier(
                GetProfileAccessIndividualByIdentifierRequest request,
                ServerCallContext context)
        {
            IAdminProfilesCreateEditViewQueryRepository.GetIndividualByIdentifierVO? individual =
                await this.serviceProvider
                    .GetRequiredService<IAdminProfilesCreateEditViewQueryRepository>()
                    .GetIndividualByIdentifierAsync(
                        request.Identifier,
                        context.CancellationToken);

            GetProfileAccessIndividualByIdentifierResponse response = new();

            if (individual != null)
            {
                response.Individual =
                    individual.Adapt<GetProfileAccessIndividualByIdentifierResponse.Types.Individual>();
            }

            return response;
        }

        public override async Task<GetProfileLoginPermissionsResponse> GetProfileLoginPermissions(
            GetProfileLoginPermissionsRequest request,
            ServerCallContext context)
        {
            IAdminProfilesCreateEditViewQueryRepository.GetLoginPermissionsVO loginPermissions =
                await this.serviceProvider
                    .GetRequiredService<IAdminProfilesCreateEditViewQueryRepository>()
                    .GetLoginPermissionsAsync(
                        request.ProfileId,
                        request.LoginId,
                        context.CancellationToken);

            return new GetProfileLoginPermissionsResponse
            {
                LoginElectronicSubjectName = loginPermissions.LoginElectronicSubjectName,
                ProfileIdentifier = loginPermissions.ProfileIdentifier,
                Permissions =
                {
                    loginPermissions.Permissions.ProjectToType<GetProfileLoginPermissionsResponse.Types.PermissionMessage>()
                }
            };
        }

        public override async Task<UpdateProfileDataResponse> UpdateProfileData(
            UpdateProfileDataRequest request,
            ServerCallContext context)
        {
            UpdateProfileDataCommandResult result = await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new UpdateProfileDataCommand(
                        request.ProfileId,
                        request.AdminUserId,
                        request.DataCase == UpdateProfileDataRequest.DataOneofCase.IndividualData
                            ? new UpdateProfileDataCommandIndividualData(
                                request.IndividualData.FirstName,
                                request.IndividualData.MiddleName,
                                request.IndividualData.LastName)
                            : null,
                        request.DataCase != UpdateProfileDataRequest.DataOneofCase.IndividualData
                            ? new UpdateProfileDataCommandLegalEntityData(
                                request.LegalEntityData.Name)
                            : null,
                        request.Identifier,
                        request.Phone,
                        request.EmailAddress,
                        request.AddressCountryCode,
                        request.AddressState,
                        request.AddressCity,
                        request.AddressResidence,
                        request.TargetGroupId,
                        request.EnableMessagesWithCode,
                        request.Ip),
                    context.CancellationToken);

            return result.Adapt<UpdateProfileDataResponse>();
        }

        public override async Task<Empty> ActivateProfile(
            ActivateProfileRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new ActivateProfileCommand(
                        request.ProfileId,
                        request.AdminUserId,
                        request.Ip),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> DeactivateProfile(
            DeactivateProfileRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new DeactivateProfileCommand(
                        request.ProfileId,
                        request.AdminUserId,
                        request.Ip),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> AddProfileRegistrationDocument(
            AddProfileRegistrationDocumentRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new AddProfileRegistrationDocumentCommand(
                        request.ProfileId,
                        request.AdminUserId,
                        request.BlobId),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> RemoveProfileRegistrationDocument(
            RemoveProfileRegistrationDocumentRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new RemoveProfileRegistrationDocumentCommand(
                        request.ProfileId,
                        request.BlobId),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> GrantProfileAccess(
            GrantOrUpdateProfileAccessRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new GrantProfileAccessByAdminCommand(
                        request.ProfileId,
                        request.LoginId,
                        request.AdminUserId,
                        request.Ip,
                        request.Permissions.ProjectToType<GrantProfileAccessByAdminCommandPermission>().ToArray()),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> UpdateProfileAccess(
            GrantOrUpdateProfileAccessRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new UpdateProfileAccessByAdminCommand(
                        request.ProfileId,
                        request.LoginId,
                        request.AdminUserId,
                        request.Ip,
                        request.Permissions.ProjectToType<UpdateProfileAccessByAdminCommandPermission>().ToArray()),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> RevokeProfileAccess(
            RevokeProfileAccessRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new RevokeProfileAccessByAdminCommand(
                        request.ProfileId,
                        request.LoginId,
                        request.AdminUserId,
                        request.Ip),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<GetIntegrationLoginInfoResponse> GetIntegrationLoginInfo(
            GetIntegrationLoginInfoRequest request,
            ServerCallContext context)
        {
            IAdminProfilesCreateEditViewQueryRepository.GetIntegrationLoginInfoVO? login =
                await this.serviceProvider
                    .GetRequiredService<IAdminProfilesCreateEditViewQueryRepository>()
                    .GetIntegrationLoginInfoAsync(
                        request.ProfileId,
                        context.CancellationToken);

            return new GetIntegrationLoginInfoResponse
            {
                Login = login!.Adapt<GetIntegrationLoginInfoResponse.Types.Login>()
            };
        }

        public override async Task<Empty> CreateOrUpdateIntegrationLogin(
            CreateOrUpdateIntegrationLoginRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new CreateOrUpdateIntegrationLoginCommand(
                        request.ProfileId,
                        request.CertificateThumbPrint,
                        request.PushNotificationsUrl,
                        request.CanSendOnBehalfOf,
                        request.SmsNotificationActive,
                        request.SmsNotificationOnDeliveryActive,
                        request.EmailNotificationActive,
                        request.EmailNotificationOnDeliveryActive,
                        request.ViberNotificationActive,
                        request.ViberNotificationOnDeliveryActive,
                        request.Email,
                        request.Phone),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> DeactivateLogin(
            DeactivateLoginRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new DeactivateLoginCommand(request.LoginId),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> ActivateLogin(
            ActivateLoginRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new ActivateLoginCommand(request.LoginId),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> MarkProfileAsReadonly(
            MarkProfileAsReadonlyRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new MarkProfileAsReadonlyCommand(
                        request.ProfileId,
                        request.AdminUserId,
                        request.Ip),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> MarkProfileAsNonReadonly(
            MarkProfileAsNonReadonlyRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new MarkProfileAsNonReadonlyCommand(
                        request.ProfileId,
                        request.AdminUserId,
                        request.Ip),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<GetProfilesByIdResponse> GetProfilesById(
            GetProfilesByIdRequest request,
            ServerCallContext context)
        {
            var profiles =
                await this.serviceProvider
                    .GetRequiredService<IAdminTemplatesCreateEditViewQueryRepository>()
                    .GetProfilesByIdAsync(
                        request.Ids.ToArray(),
                        context.CancellationToken);

            return new GetProfilesByIdResponse
            {
                Items =
                {
                    profiles.ProjectToType<GetProfilesByIdResponse.Types.Item>()
                }
            };
        }

        public override async Task<ListProfilesResponse> ListProfiles(
            ListProfilesRequest request,
            ServerCallContext context)
        {
            var profiles =
                await this.serviceProvider
                    .GetRequiredService<IAdminTemplatesCreateEditViewQueryRepository>()
                    .ListProfilesAsync(
                        request.Term,
                        request.Offset,
                        request.Limit,
                        context.CancellationToken);

            return new ListProfilesResponse
            {
                Items =
                {
                    profiles.ProjectToType<ListProfilesResponse.Types.Item>()
                }
            };
        }

        public override async Task<ListTargetGroupsResponse> ListTargetGroups(
            ListTargetGroupsRequest request,
            ServerCallContext context)
        {
            var targetGroups =
                await this.serviceProvider
                    .GetRequiredService<IAdminTemplatesCreateEditViewQueryRepository>()
                    .ListTargetGroupsAsync(
                        request.Term,
                        request.Offset,
                        request.Limit,
                        context.CancellationToken);

            return new ListTargetGroupsResponse
            {
                Items =
                {
                    targetGroups.ProjectToType<ListTargetGroupsResponse.Types.Item>()
                }
            };
        }

        public override async Task<GetTargetGroupsByIdResponse> GetTargetGroupsById(
            GetTargetGroupsByIdRequest request,
            ServerCallContext context)
        {
            IAdminNomenclaturesListQueryRepository.GetTargetGroupsByIdVO[] targetGroups =
               await this.serviceProvider
                   .GetRequiredService<IAdminNomenclaturesListQueryRepository>()
                   .GetTargetGroupsByIdAsync(
                       request.Ids.ToArray(),
                       context.CancellationToken);

            return new GetTargetGroupsByIdResponse
            {
                Items =
                {
                    targetGroups.ProjectToType<GetTargetGroupsByIdResponse.Types.Item>()
                }
            };
        }

        public override async Task<GetTemplateResponse> GetTemplate(
            GetTemplateRequest request,
            ServerCallContext context)
        {
            IAdminTemplatesCreateEditViewQueryRepository.GetVO template =
                await this.serviceProvider
                    .GetRequiredService<IAdminTemplatesCreateEditViewQueryRepository>()
                    .GetAsync(
                        request.TemplateId,
                        context.CancellationToken);

            return new GetTemplateResponse
            {
                Template = template.Adapt<GetTemplateResponse.Types.TemplateMessage>()
            };
        }

        public override async Task<GetTemplateListResponse> GetTemplateList(
            GetTemplateListRequest request,
            ServerCallContext context)
        {
            TableResultVO<IAdminTemplatesListQueryRepository.GetAllVO> templates =
                await this.serviceProvider
                    .GetRequiredService<IAdminTemplatesListQueryRepository>()
                    .GetAllAsync(
                        request.Term,
                        request.TemplateStatus.Adapt<Domain.TemplateStatus>(),
                        request.Offset,
                        request.Limit,
                        context.CancellationToken);

            return new GetTemplateListResponse
            {
                Length = templates.Length,
                Result =
                {
                    templates.Result.ProjectToType<GetTemplateListResponse.Types.TemplateMessage>()
                }
            };
        }

        public override async Task<GetTemplatePermissionsResponse> GetTemplatePermissions(
            GetTemplatePermissionsRequest request,
            ServerCallContext context)
        {
            IAdminTemplatesCreateEditViewQueryRepository.GetPermissionsVO permissions =
                await this.serviceProvider
                    .GetRequiredService<IAdminTemplatesCreateEditViewQueryRepository>()
                    .GetPermissionsAsync(
                        request.TemplateId,
                        context.CancellationToken);

            return new GetTemplatePermissionsResponse
            {
                TemplateProfiles =
                {
                    permissions.TemplateProfiles.ProjectToType<GetTemplatePermissionsResponse.Types.TemplateProfiles>()
                },
                TemplateTargetGroups =
                {
                    permissions.TemplateTargetGroups.ProjectToType<GetTemplatePermissionsResponse.Types.TemplateTargetGroups>()
                }
            };
        }

        public override async Task<CreateTemplateResponse> CreateTemplate(
            CreateTemplateRequest request,
            ServerCallContext context)
        {
            int templateId = await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new CreateTemplateCommand(
                        request.Name,
                        request.IdentityNumber,
                        request.Category,
                        request.Content,
                        request.ResponseTemplateId,
                        request.IsSystemTemplate,
                        request.CreatedByAdminUserId,
                        request.ReadLoginSecurityLevelId,
                        request.WriteLoginSecurityLevelId),
                    context.CancellationToken);

            return new CreateTemplateResponse
            {
                TemplateId = templateId
            };
        }

        public override async Task<Empty> EditTemplate(
            EditTemplateRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new EditTemplateCommand(
                        request.TemplateId,
                        request.Name,
                        request.IdentityNumber,
                        request.Category,
                        request.Content,
                        request.ResponseTemplateId,
                        request.IsSystemTemplate,
                        request.ReadLoginSecurityLevelId,
                        request.WriteLoginSecurityLevelId),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> PublishTemplate(
            PublishTemplateRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new PublishTemplateCommand(
                        request.TemplateId,
                        request.PublishedByAdminUserId),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> UnpublishTemplate(
            UnpublishTemplateRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new UnpublishTemplateCommand(
                        request.TemplateId,
                        request.PublishedByAdminUserId),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> ArchiveTemplate(
            ArchiveTemplateRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new ArchiveTemplateCommand(
                        request.TemplateId,
                        request.ArchivedByAdminUserId),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> CreateOrUpdateTemplatePermissions(
            CreateOrUpdateTemplatePermissionsRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new CreateOrUpdateTemplatePermissionsCommand(
                        request.TemplateId,
                        request.ProfileIds.ToArray(),
                        request.TargetGroupIds.ToArray(),
                        request.CanSend,
                        request.CanReceive),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> DeleteTemplateProfilePermission(
            DeleteTemplateProfilePermissionRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new DeleteTemplateProfilePermissionCommand(
                        request.TemplateId,
                        request.ProfileId),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> DeleteTemplateTargetGroupPermission(
            DeleteTemplateTargetGroupPermissionRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new DeleteTemplateTargetGroupPermissionCommand(
                        request.TemplateId,
                        request.TargetGroupId),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<RegisterProfileResponse> RegisterProfile(
            RegisterProfileRequest request,
            ServerCallContext context)
        {
            CreateLegalEntityCommandResult response =
                await this.serviceProvider
                    .GetRequiredService<IMediator>()
                    .Send(
                        new CreateLegalEntityCommand(
                            request.Name,
                            request.Identifier,
                            request.Phone,
                            request.Email,
                            request.Residence,
                            request.TargetGroupId,
                            request.BlobId,
                            request.AdminUserId,
                            request.Ip),
                        context.CancellationToken);

            return response.Adapt<RegisterProfileResponse>();
        }

        public override async Task<GetRegistrationRequestListResponse> GetRegistrationRequestList(
            GetRegistrationRequestListRequest request,
            ServerCallContext context)
        {
            TableResultVO<IAdminRegistrationsListQueryRepository.GetRegistrationRequestsVO> registrationRequests =
                await this.serviceProvider
                    .GetRequiredService<IAdminRegistrationsListQueryRepository>()
                    .GetRegistrationRequestsAsync(
                        request.AdminUserId,
                        request.Status,
                        request.Offset,
                        request.Limit,
                        context.CancellationToken);

            return new GetRegistrationRequestListResponse
            {
                Length = registrationRequests.Length,
                Result =
                {
                    registrationRequests.Result.ProjectToType<GetRegistrationRequestListResponse.Types.RegistrationRequestMessage>()
                }
            };
        }

        public override async Task<GetRegistrationRequestResponse> GetRegistrationRequest(
            GetRegistrationRequestRequest request,
            ServerCallContext context)
        {
            IAdminRegistrationsEditQueryRepository.GetRegistrationRequestVO registrationRequest =
                await this.serviceProvider
                    .GetRequiredService<IAdminRegistrationsEditQueryRepository>()
                    .GetRegistrationRequestAsync(
                        request.AdminUserId,
                        request.RegistrationRequestId,
                        context.CancellationToken);

            return registrationRequest.Adapt<GetRegistrationRequestResponse>();
        }

        public override async Task<ConfirmRegistrationRequestResponse> ConfirmRegistrationRequest(
            ConfirmRegistrationRequestRequest request,
            ServerCallContext context)
        {
            ConfirmRegistrationRequestCommandResult result =
                await this.serviceProvider
                    .GetRequiredService<IMediator>()
                    .Send(
                        new ConfirmRegistrationRequestCommand(
                            request.AdminUserId,
                            request.RegistrationRequestId,
                            request.Comment,
                            request.Ip),
                        context.CancellationToken);

            return result.Adapt<ConfirmRegistrationRequestResponse>();
        }

        public override async Task<Empty> RejectRegistrationRequest(
            RejectRegistrationRequestRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new RejectRegistrationRequestCommand(
                        request.AdminUserId,
                        request.RegistrationRequestId,
                        request.Comment,
                        request.Ip),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<ParseRegistrationDocumentResponse> ParseRegistrationDocument(
            ParseRegistrationDocumentRequest request,
            ServerCallContext context)
        {
            IAdminRegistrationsEditQueryRepository.ParseRegistrationDocumentVO document =
                await this.serviceProvider
                    .GetRequiredService<IAdminRegistrationsEditQueryRepository>()
                    .ParseRegistrationDocumentAsync(
                        request.BlobId,
                        context.CancellationToken);

            return document.Adapt<ParseRegistrationDocumentResponse>();
        }

        public override async Task<GetTargetGroupListResponse> GetTargetGroupList(
            GetTargetGroupListRequest request,
            ServerCallContext context)
        {
            TableResultVO<IAdminTargetGroupsListQueryRepository.GetAllVO> targetGroups =
                await this.serviceProvider
                   .GetRequiredService<IAdminTargetGroupsListQueryRepository>()
                   .GetAllAsync(
                        request.Offset,
                        request.Limit,
                        context.CancellationToken);

            return new GetTargetGroupListResponse
            {
                Length = targetGroups.Length,
                Result =
                {
                    targetGroups.Result.ProjectToType<GetTargetGroupListResponse.Types.TargetGroupMessage>()
                }
            };
        }

        public override async Task<GetTargetGroupResponse> GetTargetGroup(
            GetTargetGroupRequest request,
            ServerCallContext context)
        {
            IAdminTargetGroupsCreateEditViewQueryRepository.GetTargetGroupVO targetGroup =
                await this.serviceProvider
                    .GetRequiredService<IAdminTargetGroupsCreateEditViewQueryRepository>()
                    .GetTargetGroupAsync(
                        request.TargetGroupId,
                        context.CancellationToken);

            return new GetTargetGroupResponse
            {
                TargetGroup = targetGroup.Adapt<GetTargetGroupResponse.Types.TargetGroupMessage>()
            };
        }

        public override async Task<CreateTargetGroupResponse> CreateTargetGroup(
            CreateTargetGroupRequest request,
            ServerCallContext context)
        {
            int targetGroupId = await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new CreateTargetGroupCommand(
                        request.Name,
                        request.AdminUserId),
                    context.CancellationToken);

            return new CreateTargetGroupResponse
            {
                TargetGroupId = targetGroupId
            };
        }

        public override async Task<Empty> EditTargetGroup(
            EditTargetGroupRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new EditTargetGroupCommand(
                        request.TargetGroupId,
                        request.Name,
                        request.AdminUserId),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> ArchiveTargetGroup(
            ArchiveTargetGroupRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new ArchiveTargetGroupCommand(
                        request.TargetGroupId,
                        request.ArchivedByAdminUserId),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<GetTargetGroupMatrixResponse> GetTargetGroupMatrix(
            GetTargetGroupMatrixRequest request,
            ServerCallContext context)
        {
            IAdminTargetGroupsCreateEditViewQueryRepository.GetTargetGroupMatrixVO[] targetGroupMatrix =
                await this.serviceProvider
                    .GetRequiredService<IAdminTargetGroupsCreateEditViewQueryRepository>()
                    .GetTargetGroupMatrixAsync(
                        request.TargetGroupId,
                        context.CancellationToken);

            return new GetTargetGroupMatrixResponse
            {
                TargetGroups =
                {
                    targetGroupMatrix.ProjectToType<GetTargetGroupMatrixResponse.Types.TargetGroupMessage>()
                }
            };
        }

        public override async Task<Empty> InsertTargetGroupMatrix(
            InsertTargetGroupMatrixRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new InsertTargetGroupMatrixCommand(
                        request.TargetGroupId,
                        request.RecipientTargetGroupIds.ToArray(),
                        request.ArchivedByAdminUserId),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> DeleteTargetGroupMatrix(
            DeleteTargetGroupMatrixRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new DeleteTargetGroupMatrixCommand(
                        request.TargetGroupId,
                        request.RecipientTargetGroupId,
                        request.ArchivedByAdminUserId),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<CreateRecipientGroupResponse> CreateRecipientGroup(
            CreateRecipientGroupRequest request,
            ServerCallContext context)
        {
            int recipientGroupId = await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new CreateRecipientGroupCommand(
                        request.Name,
                        request.AdminUserId),
                    context.CancellationToken);

            return new CreateRecipientGroupResponse
            {
                RecipientGroupId = recipientGroupId
            };
        }

        public override async Task<Empty> EditRecipientGroup(
            EditRecipientGroupRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new EditRecipientGroupCommand(
                        request.RecipientGroupId,
                        request.Name,
                        request.AdminUserId),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> ArchiveRecipientGroup(
            ArchiveRecipientGroupRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new ArchiveRecipientGroupCommand(
                        request.RecipientGroupId,
                        request.ArchivedByAdminUserId),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> AddRecipientGroupMembers(
            AddRecipientGroupMembersRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new AddRecipientGroupMembersCommand(
                        request.RecipientGroupId,
                        request.ProfileIds.ToArray(),
                        request.ArchivedByAdminUserId),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> RemoveRecipientGroupMembers(
            RemoveRecipientGroupMembersRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new RemoveRecipientGroupMembersCommand(
                        request.RecipientGroupId,
                        request.ProfileId,
                        request.ArchivedByAdminUserId),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<GetRecipientGroupListResponse> GetRecipientGroupList(
            GetRecipientGroupListRequest request,
            ServerCallContext context)
        {
            TableResultVO<IAdminRecipientGroupsListQueryRepository.GetAllVO> recipientGroups =
                await this.serviceProvider
                   .GetRequiredService<IAdminRecipientGroupsListQueryRepository>()
                   .GetAllAsync(
                        request.Offset,
                        request.Limit,
                        context.CancellationToken);

            return new GetRecipientGroupListResponse
            {
                Length = recipientGroups.Length,
                Result =
                {
                    recipientGroups.Result.ProjectToType<GetRecipientGroupListResponse.Types.RecipientGroupMessage>()
                }
            };
        }

        public override async Task<GetRecipientGroupResponse> GetRecipientGroup(
            GetRecipientGroupRequest request,
            ServerCallContext context)
        {
            IAdminRecipientGroupsCreateEditViewQueryRepository.GetRecipientGroupVO recipientGroup =
               await this.serviceProvider
                   .GetRequiredService<IAdminRecipientGroupsCreateEditViewQueryRepository>()
                   .GetRecipientGroupAsync(
                       request.RecipientGroupId,
                       context.CancellationToken);

            return new GetRecipientGroupResponse
            {
                RecipientGroup = recipientGroup.Adapt<GetRecipientGroupResponse.Types.RecipientGroupMessage>()
            };
        }

        public override async Task<GetRecipientGroupMembersResponse> GetRecipientGroupMembers(
            GetRecipientGroupMembersRequest request,
            ServerCallContext context)
        {
            IAdminRecipientGroupsCreateEditViewQueryRepository.GetRecipientGroupMembersVO[] recipientGroupMembers =
               await this.serviceProvider
                   .GetRequiredService<IAdminRecipientGroupsCreateEditViewQueryRepository>()
                   .GetRecipientGroupMembersAsync(
                       request.RecipientGroupId,
                       context.CancellationToken);

            return new GetRecipientGroupMembersResponse
            {
                RecipientGroupMembers =
                {
                    recipientGroupMembers.ProjectToType<GetRecipientGroupMembersResponse.Types.RecipientGroupProfileMessage>()
                }
            };
        }

        public override async Task<GetAdministratorListResponse> GetAdministratorList(
            GetAdministratorListRequest request,
            ServerCallContext context)
        {
            TableResultVO<IAdminAdministratorsListQueryRepository.GetAllVO> administrators =
                await this.serviceProvider
                    .GetRequiredService<IAdminAdministratorsListQueryRepository>()
                    .GetAllAsync(
                        request.Offset,
                        request.Limit,
                        context.CancellationToken);

            return new GetAdministratorListResponse
            {
                Length = administrators.Length,
                Result =
                {
                    administrators.Result.ProjectToType<GetAdministratorListResponse.Types.AdministratorMessage>()
                }
            };
        }

        public override async Task<CreateAdministratorResponse> CreateAdministrator(
            CreateAdministratorRequest request,
            ServerCallContext context)
        {
            CreateAdministratorCommandResult result = await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new CreateAdministratorCommand(
                        request.FirstName,
                        request.MiddleName,
                        request.LastName,
                        request.Identifier,
                        request.Phone,
                        request.Email,
                        request.UserName,
                        request.PasswordHash,
                        request.AdminUserId),
                    context.CancellationToken);

            return result.Adapt<CreateAdministratorResponse>();
        }

        public override async Task<GetAdministratorResponse> GetAdministrator(
            GetAdministratorRequest request,
            ServerCallContext context)
        {
            IAdminAdministratorsCreateEditViewQueryRepository.GetAdministratorVO administrator =
                await this.serviceProvider
                    .GetRequiredService<IAdminAdministratorsCreateEditViewQueryRepository>()
                    .GetAdministratorAsync(
                        request.Id,
                        context.CancellationToken);

            return administrator.Adapt<GetAdministratorResponse>();
        }

        public override async Task<Empty> ActivateAdministrator(
            ActivateAdministratorRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new ActivateAdministratorCommand(
                        request.Id),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> DeactivateAdministrator(
            DeactivateAdministratorRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new DeactivateAdministratorCommand(
                        request.Id,
                        request.AdminUserId),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<GetAdminProfileResponse> GetAdminProfile(
            GetAdminProfileRequest request,
            ServerCallContext context)
        {
            IAdminProfileCreateEditViewQueryRepository.GetAdminProfileVO adminProfile =
                await this.serviceProvider
                    .GetRequiredService<IAdminProfileCreateEditViewQueryRepository>()
                    .GetAdminProfileAsync(
                        request.Id,
                        context.CancellationToken);

            return adminProfile.Adapt<GetAdminProfileResponse>();
        }

        public override async Task<Empty> UpdateAdminProfile(
            UpdateAdminProfileRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new UpdateAdminProfileCommand(
                        request.Id,
                        request.FirstName,
                        request.MiddleName,
                        request.LastName,
                        request.Identifier,
                        request.Phone,
                        request.Email),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> ChangePasswordAdminProfile(
            ChangePasswordAdminProfileRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new ChangePasswordAdminProfileCommand(
                        request.Id,
                        request.PasswordHash),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<GetStatisticsReportResponse> GetStatisticsReport(
            GetStatisticsReportRequest request,
            ServerCallContext context)
        {
            IAdminReportsListQueryRepository.GetStatisticsVO statistics =
                await this.serviceProvider
                    .GetRequiredService<IAdminReportsListQueryRepository>()
                    .GetStatisticsAsync(
                        request.AdminUserId,
                        context.CancellationToken);

            return statistics.Adapt<GetStatisticsReportResponse>();
        }

        public override async Task<GetDelayedMessagesReportResponse> GetDelayedMessagesReport(
            GetDelayedMessagesReportRequest request,
            ServerCallContext context)
        {
            TableResultVO<IAdminReportsListQueryRepository.GetDelayedMessagesVO> delayedMessages =
                await this.serviceProvider
                    .GetRequiredService<IAdminReportsListQueryRepository>()
                    .GetDelayedMessagesAsync(
                        request.AdminUserId,
                        request.Delay,
                        request.TargetGroupId,
                        request.ProfileId,
                        request.Offset,
                        request.Limit,
                        context.CancellationToken);

            return new GetDelayedMessagesReportResponse
            {
                Length = delayedMessages.Length,
                Result =
                {
                    delayedMessages.Result.ProjectToType<GetDelayedMessagesReportResponse.Types.DelayedMessagesMessage>()
                }
            };
        }

        public override async Task<GetEFormReportResponse> GetEFormReport(
            GetEFormReportRequest request,
            ServerCallContext context)
        {
            IAdminReportsListQueryRepository.GetEFormsVO[] eForms =
                await this.serviceProvider
                    .GetRequiredService<IAdminReportsListQueryRepository>()
                    .GetEFormsAsync(
                        request.AdminUserId,
                        request.FromDate.ToLocalDateTime(),
                        request.ToDate.ToLocalDateTime(),
                        request.Subject,
                        context.CancellationToken);

            return new GetEFormReportResponse
            {
                Result =
                {
                    eForms.ProjectToType<GetEFormReportResponse.Types.EFormMessage>()
                }
            };
        }

        public override async Task<GetNotificationsReportResponse> GetNotificationsReport(
            GetNotificationsReportRequest request,
            ServerCallContext context)
        {
            IAdminReportsListQueryRepository.GetNotificationsVO[] notifications =
                await this.serviceProvider
                    .GetRequiredService<IAdminReportsListQueryRepository>()
                    .GetNotificationsAsync(
                        request.AdminUserId,
                        request.FromDate.ToLocalDateTime(),
                        request.ToDate.ToLocalDateTime(),
                        context.CancellationToken);

            return new GetNotificationsReportResponse
            {
                Result =
                {
                    notifications.ProjectToType<GetNotificationsReportResponse.Types.NotificationsMessage>()
                }
            };
        }
        public override async Task<GetReceivedMessageReportResponse> GetReceivedMessageReport(
            GetReceivedMessageReportRequest request,
            ServerCallContext context)
        {
            TableResultVO<IAdminReportsListQueryRepository.GetReceivedMessagesVO> messages =
                await this.serviceProvider
                    .GetRequiredService<IAdminReportsListQueryRepository>()
                    .GetReceivedMessagesAsync(
                        request.AdminUserId,
                        request.FromDate.ToLocalDateTime(),
                        request.ToDate.ToLocalDateTime(),
                        request.RecipientProfileId,
                        request.SenderProfileId,
                        request.Offset,
                        request.Limit,
                        context.CancellationToken);

            return new GetReceivedMessageReportResponse
            {
                Length = messages.Length,
                Result =
                {
                    messages.Result.ProjectToType<GetReceivedMessageReportResponse.Types.MessageMessage>()
                }
            };
        }
        public override async Task<GetSentMessageReportResponse> GetSentMessageReport(
            GetSentMessageReportRequest request,
            ServerCallContext context)
        {
            TableResultVO<IAdminReportsListQueryRepository.GetSentMessagesVO> messages =
                await this.serviceProvider
                    .GetRequiredService<IAdminReportsListQueryRepository>()
                    .GetSentMessagesAsync(
                        request.AdminUserId,
                        request.FromDate.ToLocalDateTime(),
                        request.ToDate.ToLocalDateTime(),
                        request.RecipientProfileId,
                        request.SenderProfileId,
                        request.Offset,
                        request.Limit,
                        context.CancellationToken);

            return new GetSentMessageReportResponse
            {
                Length = messages.Length,
                Result =
                {
                    messages.Result.ProjectToType<GetSentMessageReportResponse.Types.MessageMessage>()
                }
            };
        }

        public override async Task<GetTicketsReportResponse> GetTicketsReport(
            GetTicketsReportRequest request,
            ServerCallContext context)
        {
            IAdminReportsListQueryRepository.GetTicketsVO tickets =
                await this.serviceProvider
                    .GetRequiredService<IAdminReportsListQueryRepository>()
                    .GetTicketsAsync(
                        request.AdminUserId,
                        request.From.ToLocalDateTime(),
                        request.To.ToLocalDateTime(),
                        context.CancellationToken);

            return tickets.Adapt<GetTicketsReportResponse>();
        }

        public async override Task<GetSeosParticipantsListResponse> GetSeosParticipantsList(
            GetSeosParticipantsListRequest request,
            ServerCallContext context)
        {
            List<IAdminSeosParticipantsListQueryRepository.GetSeosParticipantsQO> seosParticipants =
                await this.serviceProvider
                    .GetRequiredService<IAdminSeosParticipantsListQueryRepository>()
                    .GetSeosParticipantsAsync(
                        request.Offset,
                        request.Limit,
                        context.CancellationToken);

            return new GetSeosParticipantsListResponse
            {
                Length = seosParticipants.Count,
                Result =
                {
                    seosParticipants.ProjectToType<GetSeosParticipantsListResponse.Types.SeosParticipantMessage>()
                }
            };
        }

        public override async Task<GetRegisteredEntitiesResponse> GetRegisteredEntities(
            GetRegisteredEntitiesRequest request,
            ServerCallContext context)
        {
            List<IAdminSeosParticipantsCreateQueryRepository.GetRegisteredEntitiesQO> registeredEntities =
                await this.serviceProvider
                    .GetRequiredService<IAdminSeosParticipantsCreateQueryRepository>()
                    .GetRegisteredEntitiesAsync(
                        request.Offset,
                        request.Limit,
                        context.CancellationToken);

            return new GetRegisteredEntitiesResponse()
            {
                Length = registeredEntities.Count,
                Result =
                {
                    registeredEntities.ProjectToType<GetRegisteredEntitiesResponse.Types.RegisteredEntityMessage>()
                }
            };
        }

        public override async Task<Empty> CreateSeosParticipant(
            CreateSeosParticipantRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new CreateSeosParticipantCommand(
                        request.RegisteredEntityIdentifier,
                        request.As4Node),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<Empty> DeleteSeosParticipant(
            DeleteSeosParticipantRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
                .GetRequiredService<IMediator>()
                .Send(
                    new DeleteSeosParticipantCommand(
                        request.ParticipantId),
                    context.CancellationToken);

            return new Empty();
        }

        public override async Task<GetLoginProfileNotificationsResponse> GetLoginProfileNotifications(
            GetLoginProfileNotificationsRequest request,
            ServerCallContext context)
        {
            IAdminProfilesCreateEditViewQueryRepository.GetLoginProfileNotificationSettingsVO notifications =
                await this.serviceProvider
                    .GetRequiredService<IAdminProfilesCreateEditViewQueryRepository>()
                    .GetLoginProfileNotificationSettingsAsync(
                        request.LoginId,
                        request.ProfileId,
                        context.CancellationToken);

            return notifications.Adapt<GetLoginProfileNotificationsResponse>();
        }

        public override async Task<Empty> UpdateLoginProfileNotifications(
            UpdateLoginProfileNotificationsRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
               .GetRequiredService<IMediator>()
               .Send(
                   new UpdateLoginProfileNotificationsByAdminCommand(
                       request.ProfileId,
                       request.LoginId,
                       request.Email,
                       request.Phone,
                       request.EmailNotificationActive,
                       request.EmailNotificationOnDeliveryActive,
                       request.SmsNotificationActive,
                       request.SmsNotificationOnDeliveryActive,
                       request.ViberNotificationActive,
                       request.ViberNotificationOnDeliveryActive,
                       request.AdminUserId,
                       request.Ip),
                   context.CancellationToken);

            return new Empty();
        }

        public override async Task<GetTimestampsReportResponse> GetTimestampsReport(
            GetTimestampsReportRequest request,
            ServerCallContext context)
        {
            IAdminReportsListQueryRepository.GetTimestampsVO timestamps =
                await this.serviceProvider
                    .GetRequiredService<IAdminReportsListQueryRepository>()
                    .GetTimestampsAsync(
                        request.AdminUserId,
                        request.FromDate.ToLocalDateTime(),
                        request.ToDate.ToLocalDateTime(),
                        context.CancellationToken);

            return timestamps.Adapt<GetTimestampsReportResponse>();
        }

        public override async Task<GetProfileQuotasInfoResponse> GetProfileQuotasInfo(
            GetProfileQuotasInfoRequest request,
            ServerCallContext context)
        {
            IAdminProfilesCreateEditViewQueryRepository.GetProfileQuotasInfoVO quotas =
                await this.serviceProvider
                    .GetRequiredService<IAdminProfilesCreateEditViewQueryRepository>()
                    .GetProfileQuotasInfoAsync(
                        request.ProfileId,
                        context.CancellationToken);

            return quotas.Adapt<GetProfileQuotasInfoResponse>();
        }

        public override async Task<Empty> UpdateProfileQuotas(
            UpdateProfileQuotasRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
               .GetRequiredService<IMediator>()
               .Send(
                   new CreateOrUpdateProfileQuotasCommand(
                       request.ProfileId,
                       request.AdminUserId,
                       request.StorageQuotaInMb,
                       request.Ip),
                   context.CancellationToken);

            return new Empty();
        }

        public override async Task<GetProfileEsbUserInfoResponse> GetProfileEsbUserInfo(
            GetProfileEsbUserInfoRequest request,
            ServerCallContext context)
        {
            IAdminProfilesCreateEditViewQueryRepository.GetProfileEsbUserInfoVO esbUser =
                await this.serviceProvider
                    .GetRequiredService<IAdminProfilesCreateEditViewQueryRepository>()
                    .GetProfileEsbUserInfoAsync(
                        request.ProfileId,
                        context.CancellationToken);

            return esbUser.Adapt<GetProfileEsbUserInfoResponse>();
        }

        public override async Task<Empty> UpdateProfileEsbUser(
            UpdateProfileEsbUserRequest request,
            ServerCallContext context)
        {
            await this.serviceProvider
               .GetRequiredService<IMediator>()
               .Send(
                   new CreateOrUpdateProfileEsbUserCommand(
                       request.ProfileId,
                       request.OId,
                       request.ClientId,
                       request.AdminUserId),
                   context.CancellationToken);

            return new Empty();
        }

        public override async Task<GetProfileHistoryResponse> GetProfileHistory(
            GetProfileHistoryRequest request,
            ServerCallContext context)
        {
            TableResultVO<IAdminProfilesCreateEditViewQueryRepository.GetHistoryVO> history =
                await this.serviceProvider
                    .GetRequiredService<IAdminProfilesCreateEditViewQueryRepository>()
                    .GetHistoryAsync(
                        request.ProfileId,
                        request.Actions.ProjectToType<Domain.ProfileHistoryAction>().ToArray(),
                        request.Offset,
                        request.Limit,
                        context.CancellationToken);

            return new GetProfileHistoryResponse
            {
                Length = history.Length,
                Result =
                {
                    history.Result.ProjectToType<GetProfileHistoryResponse.Types.History>()
                }
            };
        }
    }
}
