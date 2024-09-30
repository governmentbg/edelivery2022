using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Mvc;

using ED.DomainServices.Journals;
using ED.DomainServices.Profiles;

using EDelivery.WebPortal.Authorization;
using EDelivery.WebPortal.Enums;
using EDelivery.WebPortal.Extensions;
using EDelivery.WebPortal.Models;
using EDelivery.WebPortal.Models.Profile;
using EDelivery.WebPortal.Models.Profile.Administration;
using EDelivery.WebPortal.Utils;
using EDelivery.WebPortal.Utils.Attributes;
using EDelivery.WebPortal.Utils.Cache;

using EDeliveryResources;

namespace EDelivery.WebPortal.Controllers
{
    [EDeliveryResourceAuthorize]
    public class ProfileController : BaseController
    {
        private readonly Lazy<Profile.ProfileClient> profileClient;
        private readonly Lazy<Journal.JournalClient> journalClient;
        public static readonly Lazy<int> MaxRecipientGroups =
            new Lazy<int>(() => int.Parse(WebConfigurationManager.AppSettings["MaxRecipientGroups"]));
        public static readonly Lazy<int> MaxRecipientGroupMembers =
            new Lazy<int>(() => int.Parse(WebConfigurationManager.AppSettings["MaxRecipientGroupMembers"]));

        public ProfileController()
        {
            this.profileClient = new Lazy<Profile.ProfileClient>(
                () => Grpc.GrpcClientFactory.CreateProfileClient(), isThreadSafe: false);

            this.journalClient = new Lazy<Journal.JournalClient>(
                () => Grpc.GrpcClientFactory.CreateJournalClient(), isThreadSafe: false);
        }

        [HttpGet]
        [BreadCrumb(2, "Създаване на профил", eLeftMenu.CreateProfile)]
        public async Task<ActionResult> CreateProfile()
        {
            if (!this.UserData.ActiveProfile.IsPassive)
            {
                return RedirectToAction(nameof(ProfileController.Index));
            }

            CreateProfileViewModel cm = this.GetTempModel<CreateProfileViewModel>(true);

            if (cm != null)
            {
                return View(cm);
            }

            GetPassiveProfileDataResponse parsedResponse =
                await this.profileClient.Value.GetPassiveProfileDataAsync(
                    new GetPassiveProfileDataRequest
                    {
                        LoginId = this.UserData.LoginId
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            CreateProfileViewModel vm = new CreateProfileViewModel
            {
                FirstName = parsedResponse.FirstName,
                MiddleName = parsedResponse.MiddleName,
                LastName = parsedResponse.LastName,
                EmailAddress = parsedResponse.Email,
                Address = parsedResponse.Address,
                PhoneNumber = parsedResponse.Phone
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateProfile(CreateProfileViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    this.SetTempModel(model, true);

                    return RedirectToAction(
                        nameof(ProfileController.CreateProfile));
                }

                await this.profileClient.Value.BringProfileInForceAsync(
                    new BringProfileInForceRequest
                    {
                        Ip = this.Request.UserHostAddress,
                        IsEmailNotificationEnabled = model.IsEmailNotificationEnabled,
                        IsEmailNotificationOnDeliveryEnabled = false,
                        IsPhoneNotificationEnabled = model.IsPhoneNotificationEnabled,
                        IsPhoneNotificationOnDeliveryEnabled = false,
                        LoginId = this.UserData.LoginId
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

                this.HttpContext.ClearCachedUserData();

                return RedirectToAction(nameof(ProfileController.Index));
            }
            catch (Exception ex)
            {
                ElmahLogger.Instance.Error(
                    ex,
                    "Unsuccessful creation of profile");

                ModelState.AddModelError(
                    string.Empty,
                    ErrorMessages.ErrorSystemGeneral);

                this.SetTempModel(model, true);

                return RedirectToAction(
                    nameof(ProfileController.CreateProfile));
            }
        }

        [HttpGet]
        [BreadCrumb(2, typeof(EDeliveryResources.Common), "TitleProfileHome", eLeftMenu.None, true)]
        public async Task<ActionResult> Index()
        {
            if (this.UserData.ActiveProfile.IsPassive)
            {
                return View("NoProfile");
            }

            _ = await this.profileClient.Value.CreateAccessProfilesHistoryAsync(
                new CreateAccessProfilesHistoryRequest
                {
                    ProfileId = this.UserData.ActiveProfileId,
                    LoginId = this.UserData.LoginId,
                    Ip = this.Request.UserHostAddress
                },
                cancellationToken: Response.ClientDisconnectedToken);

            ProfileModel model = new ProfileModel(this.UserData.ActiveProfile);

            return View(model);
        }

        [HttpGet]
        public ActionResult SwitchActiveProfile(int profileId)
        {
            this.UserData.ActiveProfileId = profileId;
            this.UserData.BreadCrumb.ProfileName =
                this.UserData.ActiveProfile.ProfileName;

            return RedirectToAction("Index");
        }

        [HttpGet]
        [BreadCrumb(2, typeof(EDeliveryResources.Common), "TitleProfileAdministration", eLeftMenu.Administration)]
        public ActionResult Administer()
        {
            return View();
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.AdministerProfile)]
        [HttpGet]
        [BreadCrumb(3, "Профилни групи", eLeftMenu.Administration)]
        public async Task<ActionResult> Groups(int page = 1)
        {
            GetRecipientGroupsResponse response =
                await profileClient.Value.GetRecipientGroupsAsync(
                    new GetRecipientGroupsRequest
                    {
                        ProfileId = this.UserData.ActiveProfileId,
                        Offset = (page - 1) * SystemConstants.PageSize,
                        Limit = SystemConstants.PageSize,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            List<RecipientGroupItemViewModel> items = response
                .RecipientGroups
                .Select(x => new RecipientGroupItemViewModel(x))
                .ToList();

            PagedList.PagedListLight<RecipientGroupItemViewModel> model =
                new PagedList.PagedListLight<RecipientGroupItemViewModel>(
                    items,
                    SystemConstants.PageSize,
                    page,
                    response.Length);

            if (Request.IsAjaxRequest())
            {
                return PartialView("Partials/Groups", model);
            }

            return View(model);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.AdministerProfile)]
        [HttpGet]
        [BreadCrumb(4, "Създаване", eLeftMenu.Administration)]
        public async Task<ActionResult> CreateGroup()
        {
            if (this.UserData.ActiveProfile.TargetGroupId == (int)TargetGroupId.Individual)
            {
                GetRecipientGroupsCountResponse groups =
                    await profileClient.Value.GetRecipientGroupsCountAsync(
                        new GetRecipientGroupsCountRequest
                        {
                            ProfileId = this.UserData.ActiveProfileId
                        });

                if (groups.NumberOfRecipientGroups >= MaxRecipientGroups.Value)
                {
                    return RedirectToAction(nameof(ProfileController.Groups));
                }
            }

            CreateRecipientGroupViewModel vm =
                this.GetTempModel<CreateRecipientGroupViewModel>(true)
                    ?? new CreateRecipientGroupViewModel(this.UserData.ActiveProfileId);

            return View(vm);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.AdministerProfile)]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> CreateGroup(
            CreateRecipientGroupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                this.SetTempModel(model, true);

                return RedirectToAction(nameof(ProfileController.CreateGroup));
            }

            if (model.ProfileId != this.UserData.ActiveProfileId)
            {
                throw new Exception("Profile sync failed");
            }

            CreateRecipientGroupResponse response =
                await profileClient.Value.CreateRecipientGroupAsync(
                    new CreateRecipientGroupRequest
                    {
                        Name = model.Name,
                        ProfileId = this.UserData.ActiveProfileId,
                        LoginId = this.UserData.LoginId
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            if (!response.IsSuccessful)
            {
                ModelState.AddModelError(
                        string.Empty,
                        ErrorMessages.ErrorExistingGroupName);

                this.SetTempModel(model, true);

                return RedirectToAction(nameof(ProfileController.CreateGroup));
            }

            return RedirectToAction(
                nameof(ProfileController.GroupDetails),
                new { id = response.RecipientGroupId });
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(
            Policy = Policies.AdministerProfileRecipientGroups,
            RecipientGroupIdRouteOrQueryParam = "id")]
        [HttpGet]
        [BreadCrumb(4, "Преглед", eLeftMenu.Administration)]
        public async Task<ActionResult> GroupDetails(
            [Bind(Prefix = "id")] int recipientGroupId)
        {
            GetRecipientGroupResponse response =
                await profileClient.Value.GetRecipientGroupAsync(
                    new GetRecipientGroupRequest
                    {
                        RecipientGroupId = recipientGroupId,
                        ProfileId = this.UserData.ActiveProfileId
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            RecipientGroupViewModel vm =
                new RecipientGroupViewModel(response);

            if (Request.IsAjaxRequest())
            {
                return PartialView("Partials/GroupDetails", vm);
            }

            return View(vm);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(
            Policy = Policies.AdministerProfileRecipientGroups,
            RecipientGroupIdRouteOrQueryParam = "id")]
        [ChildActionOnlyOrAjax]
        [HttpGet]
        public ActionResult GroupMembers(
            [Bind(Prefix = "id")] int recipientGroupId)
        {
            GetRecipientGroupMembersResponse response =
                profileClient.Value.GetRecipientGroupMembers(
                    new GetRecipientGroupMembersRequest
                    {
                        RecipientGroupId = recipientGroupId,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            RecipientGroupMembersViewModel vm =
                new RecipientGroupMembersViewModel(
                    recipientGroupId,
                    response.Members.ToArray());

            return PartialView("Partials/GroupMembers", vm);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(
            Policy = Policies.AdministerProfileRecipientGroups,
            RecipientGroupIdRouteOrQueryParam = "id")]
        [ChildActionOnlyOrAjax]
        [HttpGet]
        public async Task<ActionResult> AddGroupMember(
            [Bind(Prefix = "id")] int recipientGroupId)
        {
            if (this.UserData.ActiveProfile.TargetGroupId == (int)TargetGroupId.Individual)
            {
                GetRecipientGroupMembersResponse members =
                    profileClient.Value.GetRecipientGroupMembers(
                        new GetRecipientGroupMembersRequest
                        {
                            RecipientGroupId = recipientGroupId,
                        });

                if (members.Length >= MaxRecipientGroupMembers.Value)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }

            GetTargetGroupsResponse targetGroups =
                await this.profileClient.Value.GetTargetGroupsAsync(
                    new GetTargetGroupsRequest
                    {
                        ProfileId = this.UserData.ActiveProfileId
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            AddRecipientGroupMemberViewModel vm =
                new AddRecipientGroupMemberViewModel(recipientGroupId)
                {
                    CanSendToIndividuals = targetGroups
                        .Result
                        .Any(e => e.TargetGroupId == (int)TargetGroupId.Individual),
                    CanSendToLegalEntities = targetGroups
                        .Result
                        .Any(e => e.TargetGroupId == (int)TargetGroupId.LegalEntity),
                    TargetGroups = new SelectListItem[]
                    {
                        new SelectListItem
                        {
                            Text = Common.OptionAll,
                            Value = null,
                            Selected = true
                        }
                    }
                    .Concat(
                        targetGroups.Result
                            .Where(e => e.TargetGroupId != (int)TargetGroupId.Individual
                                && e.TargetGroupId != (int)TargetGroupId.LegalEntity)
                            .Select(e => new SelectListItem
                            {
                                Text = e.Name,
                                Value = e.TargetGroupId.ToString()
                            }))
                    .ToList(),
                };

            return PartialView("Modals/AddGroupMember", vm);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(
            Policy = Policies.AdministerProfileRecipientGroups,
            RecipientGroupIdRouteOrQueryParam = "id")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> AddGroupMember(
            [Bind(Prefix = "id")] int recipientGroupId,
            AddRecipientGroupMemberPostViewModel model)
        {
            int[] profileIds = model.ProfileIds?
                .Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(e => int.Parse(e))
                .ToArray()
                    ?? Array.Empty<int>();

            if (profileIds.Any())
            {
                _ = await this.profileClient.Value.UpdateRecipientGroupMembersAsync(
                    new UpdateRecipientGroupMembersRequest
                    {
                        RecipientGroupId = model.RecipientGroupId,
                        ProfileIds = { profileIds },
                        LoginId = this.UserData.LoginId,
                        ProfileId = this.UserData.ActiveProfile.ProfileId
                    },
                    cancellationToken: Response.ClientDisconnectedToken);
            }

            return RedirectToAction(
                nameof(ProfileController.GroupDetails),
                new { id = recipientGroupId });
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.SearchMessageRecipientIndividuals)]
        [HttpPost]
        public async Task<ActionResult> FindIndividual(
            AddRecipientGroupMemberIndividualViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("Modals/AddGroupMemberIndividual", model);
            }

            FindIndividualResponse individual =
                await this.profileClient.Value.FindIndividualAsync(
                    new FindIndividualRequest
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Identifier = model.Identifier
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            if (individual.Individual == null)
            {
                ModelState.AddModelError(
                    nameof(model.FirstName),
                    ErrorMessages.ErrorPersonNotFound);

                return PartialView("Modals/AddGroupMemberIndividual", model);
            }

            ModelState.Clear();

            AddRecipientGroupMemberIndividualViewModel vm =
                new AddRecipientGroupMemberIndividualViewModel
                {
                    SelectedIndividualProfileId = individual.Individual.ProfileId,
                    SelectedIndividualProfileName = individual.Individual.Name
                };

            return PartialView("Modals/AddGroupMemberIndividual", vm);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.SearchMessageRecipientLegalEntities)]
        [HttpPost]
        public async Task<ActionResult> FindLegalEntity(
            AddRecipientGroupMemberLegalEntityViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("Modals/AddGroupMemberLegalEntity");
            }

            FindLegalEntityResponse legalEntity =
                await this.profileClient.Value.FindLegalEntityAsync(
                    new FindLegalEntityRequest
                    {
                        Identifier = model.CompanyRegistrationNumber
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            if (legalEntity.LegalEntity == null)
            {
                ModelState.AddModelError(
                    nameof(model.CompanyRegistrationNumber),
                    ErrorMessages.ErrorLegalNotFound);

                return PartialView("Modals/AddGroupMemberLegalEntity");
            }

            ModelState.Clear();

            AddRecipientGroupMemberLegalEntityViewModel vm =
                new AddRecipientGroupMemberLegalEntityViewModel
                {
                    SelectedLegalEntityProfileId = legalEntity.LegalEntity.ProfileId,
                    SelectedLegalEntityProfileName = legalEntity.LegalEntity.Name
                };

            return PartialView("Modals/AddGroupMemberLegalEntity", vm);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(
            Policy = Policies.AdministerProfileRecipientGroups,
            RecipientGroupIdRouteOrQueryParam = "recipientGroupId")]
        [HttpPost]
        public async Task<ActionResult> RemoveGroupMember(
            RemoveRecipientGroupMemberViewModel model)
        {
            _ = await this.profileClient.Value.DeleteRecipientGroupMemberAsync(
                new DeleteRecipientGroupMemberRequest
                {
                    RecipientGroupId = model.RecipientGroupId,
                    ProfileId = model.ProfileId,
                    LoginId = this.UserData.LoginId
                },
                cancellationToken: Response.ClientDisconnectedToken);

            return RedirectToAction(
                nameof(ProfileController.GroupMembers),
                new { id = model.RecipientGroupId });
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(
            Policy = Policies.AdministerProfileRecipientGroups,
            RecipientGroupIdRouteOrQueryParam = "id")]
        [HttpGet]
        public async Task<ActionResult> EditGroup(
            [Bind(Prefix = "id")] int recipientGroupId)
        {
            GetRecipientGroupResponse response =
                await profileClient.Value.GetRecipientGroupAsync(
                    new GetRecipientGroupRequest
                    {
                        RecipientGroupId = recipientGroupId,
                        ProfileId = this.UserData.ActiveProfileId
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            EditRecipientGroupViewModel vm =
                this.GetTempModel<EditRecipientGroupViewModel>(true)
                    ?? new EditRecipientGroupViewModel(response);

            return View(vm);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(
            Policy = Policies.AdministerProfileRecipientGroups,
            RecipientGroupIdRouteOrQueryParam = "id")]
        [HttpPost]
        public async Task<ActionResult> EditGroup(
            [Bind(Prefix = "id")] int recipientGroupId,
            EditRecipientGroupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                this.SetTempModel(model, true);

                return RedirectToAction(
                    nameof(ProfileController.EditGroup),
                    new { id = recipientGroupId });
            }

            _ = await this.profileClient.Value.UpdateRecipientGroupAsync(
                new UpdateRecipientGroupRequest
                {
                    RecipientGroupId = recipientGroupId,
                    Name = model.Name,
                    LoginId = this.UserData.LoginId
                },
                cancellationToken: Response.ClientDisconnectedToken);

            return RedirectToAction(
                nameof(ProfileController.GroupDetails),
                new { id = recipientGroupId });
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(
            Policy = Policies.AdministerProfileRecipientGroups,
            RecipientGroupIdRouteOrQueryParam = "id")]
        [HttpPost]
        public async Task<ActionResult> ArchiveGroup(
            [Bind(Prefix = "id")] int recipientGroupId)
        {
            _ = await this.profileClient.Value.ArchiveRecipientGroupAsync(
                new ArchiveRecipientGroupRequest
                {
                    RecipientGroupId = recipientGroupId,
                    LoginId = this.UserData.LoginId
                },
                cancellationToken: Response.ClientDisconnectedToken);

            return RedirectToAction(nameof(ProfileController.Groups));
        }

        [HttpGet]
        [BreadCrumb(3, typeof(Common), "TitleSettings", eLeftMenu.Administration)]
        public async Task<ActionResult> Settings()
        {
            GetSettingsResponse response =
                await this.profileClient.Value.GetSettingsAsync(
                    new GetSettingsRequest
                    {
                        ProfileId = this.UserData.ActiveProfileId,
                        LoginId = this.UserData.LoginId,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            LoginProfileSettingsViewModel model =
                new LoginProfileSettingsViewModel(
                    this.UserData.ActiveProfileId,
                    this.UserData.ActiveProfile.TargetGroupId,
                    response);

            return View(model);
        }

        // TODO: even if it fails the js alert is for success
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateSettings(
            LoginProfileSettingsViewModel model)
        {
            try
            {
                if (model.ProfileId != this.UserData.ActiveProfileId)
                {
                    throw new Exception("Profile sync error");
                }

                if (!ModelState.IsValid)
                {
                    ModelState.AddModelError("", ErrorMessages.ErrorInvalidData);

                    return PartialView("Partials/_NotificationSettings", model);
                }

                _ = await this.profileClient.Value.UpdateSettingsAsync(
                    new UpdateSettingsRequest
                    {
                        ProfileId = this.UserData.ActiveProfileId,
                        LoginId = this.UserData.LoginId,
                        IsEmailNotificationEnabled = model.IsEmailNotificationEnabled,
                        IsEmailNotificationOnDeliveryEnabled = model.IsEmailNotificationOnDeliveryEnabled,
                        IsPhoneNotificationEnabled = model.IsPhoneNotificationEnabled,
                        IsPhoneNotificationOnDeliveryEnabled = model.IsPhoneNotificationOnDeliveryEnabled,
                        Email = model.Email,
                        Phone = model.Phone,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

                return PartialView("Partials/_NotificationSettings", model);
            }
            catch (Exception ex)
            {
                ElmahLogger.Instance.Error(
                    ex,
                    $"Error updating notification settings for login {UserData.LoginId} and profile id: {model.ProfileId}");

                ModelState.AddModelError("", ErrorMessages.ErrorSystemGeneral);

                return PartialView("Partials/_NotificationSettings", model);
            }
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.AdministerProfile)]
        [HttpGet]
        [BreadCrumb(3, "Конфигурация", eLeftMenu.Administration)]
        public ActionResult Administration()
        {
            return View();
        }

        [HttpGet]
        [ChildActionOnly]
        public ActionResult ProfileAttachedDocuments()
        {
            GetBlobsResponse blobs =
                this.profileClient.Value.GetBlobs(
                    new GetBlobsRequest
                    {
                        ProfileId = this.UserData.ActiveProfileId
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            IEnumerable<ProfileBlobsViewModel> vm = blobs
                .Result
                .Select(e => new ProfileBlobsViewModel(e));

            return PartialView("Partials/_ProfileBlobs", vm);
        }

        #region Access to profile

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.AdministerProfile)]
        [HttpGet]
        [ChildActionOnlyOrAjax]
        public ActionResult ProfileAccess()
        {
            if (this.UserData.ActiveProfile.TargetGroupId == (int)TargetGroupId.Individual)
            {
                return Content("");
            }

            GetLoginsResponse response =
                this.profileClient.Value.GetLogins(
                    new GetLoginsRequest
                    {
                        ProfileId = this.UserData.ActiveProfileId,
                        LoginId = this.UserData.LoginId,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            ProfileAccessViewModel model =
                new ProfileAccessViewModel(
                    this.UserData.ActiveProfileId,
                    response.Logins.ToArray());

            return PartialView("Partials/_ProfileAccess", model);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.AdministerProfile)]
        [HttpGet]
        public ActionResult GrantAccess(int profileId)
        {
            if (profileId != this.UserData.ActiveProfileId)
            {
                throw new Exception("Profile sync error");
            }

            GrantAccessViewModel model = new GrantAccessViewModel()
            {
                ProfileId = profileId
            };

            return PartialView("Partials/GrantAccess", model);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.AdministerProfile)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GrantAccess(GrantAccessViewModel model)
        {
            try
            {
                if (model.ProfileId != this.UserData.ActiveProfileId)
                {
                    throw new Exception("Profile sync error");
                }

                if (!ModelState.IsValid)
                {
                    return PartialView("Partials/GrantAccess", model);
                }

                bool isCurrentProfilePublicAdministration = this.UserData
                    .Profiles
                    .First(x => x.ProfileId == model.ProfileId)
                    .TargetGroupId == (int)TargetGroupId.PublicAdministration;

                FindLoginResponse response =
                    await this.profileClient.Value.FindLoginAsync(
                        new FindLoginRequest
                        {
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            Identifier = model.Identifier,
                        },
                        cancellationToken: Response.ClientDisconnectedToken);

                GetTemplatesResponse templates =
                    await this.profileClient.Value.GetTemplatesAsync(
                        new GetTemplatesRequest
                        {
                            ProfileId = this.UserData.ActiveProfileId
                        },
                        cancellationToken: Response.ClientDisconnectedToken);

                if (response.Login == null)
                {
                    ModelState.AddModelError(
                        nameof(model.Identifier),
                        ErrorMessages.ErrorPersonNotFound);

                    return PartialView("Partials/GrantAccess", model);
                }

                AddLoginProfilePermissionsViewModel vm =
                    new AddLoginProfilePermissionsViewModel(
                        response.Login.LoginId,
                        model.ProfileId,
                        response.Login.LoginName,
                        response.Login.ProfileIdentifier,
                        true,
                        false,
                        !isCurrentProfilePublicAdministration,
                        false,
                        templates.Result.ToArray());

                return PartialView("Partials/AddLoginProfilePermissions", vm);
            }
            catch (Exception ex)
            {
                ElmahLogger.Instance.Error(ex, "Can not grant access for person" + model.Identifier);

                ModelState.AddModelError(
                    string.Empty,
                    ErrorMessages.ErrorSystemGeneral);

                return PartialView("Partials/GrantAccess", model);
            }
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.AdministerProfile)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RevokeAccess(
            int loginId,
            int profileId)
        {
            try
            {
                if (profileId != this.UserData.ActiveProfileId)
                {
                    throw new Exception("Profile sync error");
                }

                _ = await this.profileClient.Value.RevokeAccessAsync(
                    new RevokeAccessRequest
                    {
                        ProfileId = profileId,
                        LoginId = loginId,
                        ActionLoginId = this.UserData.LoginId,
                        Ip = this.Request.UserHostAddress
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

                return RedirectToAction("ProfileAccess");
            }
            catch (Exception ex)
            {
                ElmahLogger.Instance.Error(
                    ex,
                    $"Can not remove access to profile for login {loginId}");

                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.AdministerProfile)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddLoginProfilePermissions(
            AddLoginProfilePermissionsViewModel model)
        {
            if (model.ProfileId != this.UserData.ActiveProfileId)
            {
                throw new Exception("Profile sync error");
            }

            List<GrantAccessRequest.Types.PermissionMessage> permissions =
                model.ExtractPermissions();

            _ = await this.profileClient.Value.GrantAccessAsync(
                new GrantAccessRequest
                {
                    ProfileId = model.ProfileId,
                    LoginId = model.LoginId,
                    IsDefault = false,
                    IsEmailNotificationEnabled = model.IsEmailNotificationEnabled,
                    IsEmailNotificationOnDeliveryEnabled = model.IsEmailNotificationOnDeliveryEnabled,
                    IsPhoneNotificationEnabled = model.IsPhoneNotificationEnabled,
                    IsPhoneNotificationOnDeliveryEnabled = model.IsPhoneNotificationOnDeliveryEnabled,
                    Details = string.Empty,
                    ActionLoginId = this.UserData.LoginId,
                    Ip = this.Request.UserHostAddress,
                    Permissions = { permissions },
                },
                cancellationToken: Response.ClientDisconnectedToken);

            return RedirectToAction("ProfileAccess");
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.AdministerProfile)]
        [HttpGet]
        public async Task<ActionResult> UpdateLoginProfilePermissions(
            int loginId)
        {
            GetLoginPermissionsResponse loginPermissions =
                await this.profileClient.Value.GetLoginPermissionsAsync(
                    new GetLoginPermissionsRequest
                    {
                        ProfileId = this.UserData.ActiveProfileId,
                        LoginId = loginId
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            GetTemplatesResponse templates =
                await this.profileClient.Value.GetTemplatesAsync(
                    new GetTemplatesRequest
                    {
                        ProfileId = this.UserData.ActiveProfileId
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            UpdateLoginProfilePermissionsViewModel vm =
                new UpdateLoginProfilePermissionsViewModel(
                    loginId,
                    this.UserData.ActiveProfileId,
                    loginPermissions,
                    templates.Result.ToArray());

            return PartialView("Partials/UpdateLoginProfilePermissions", vm);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.AdministerProfile)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateLoginProfilePermissions(
            UpdateLoginProfilePermissionsViewModel model)
        {
            if (model.ProfileId != this.UserData.ActiveProfileId)
            {
                throw new Exception("Profile sync error");
            }

            List<UpdateAccessRequest.Types.PermissionMessage> permissions =
                model.ExtractPermissions();

            _ = await this.profileClient.Value.UpdateAccessAsync(
                new UpdateAccessRequest
                {
                    ProfileId = model.ProfileId,
                    LoginId = model.LoginId,
                    Details = string.Empty,
                    ActionLoginId = this.UserData.LoginId,
                    Ip = this.Request.UserHostAddress,
                    Permissions = { permissions }
                },
                cancellationToken: Response.ClientDisconnectedToken);

            return RedirectToAction("ProfileAccess");
        }

        #endregion Access to profile

        [HttpGet]
        [ChildActionOnly]
        public ActionResult ProfileInfo()
        {
            TargetGroupId targetGroupId =
                (TargetGroupId)UserData.ActiveProfile.TargetGroupId;

            // TODO: remake with other targetgroups
            switch (targetGroupId)
            {
                case TargetGroupId.Individual:
                    GetIndividualResponse r1 =
                        this.profileClient.Value.GetIndividual(
                            new GetIndividualRequest
                            {
                                ProfileId = this.UserData.ActiveProfileId
                            },
                            cancellationToken: Response.ClientDisconnectedToken);

                    PersonProfileModel m1 = new PersonProfileModel()
                    {
                        Editable = new CommonDataModel(
                            UserData.ActiveProfile.ProfileId,
                            UserData.ActiveProfile.TargetGroupId,
                            r1.Email,
                            r1.Phone,
                            r1.Residence,
                            true),
                        Request = new PersonSpecificModel(
                            r1.FirstName,
                            r1.MiddleName,
                            r1.LastName,
                            r1.Identifier)
                    };

                    return PartialView("Partials/_ProfileInfo", m1);
                case TargetGroupId.LegalEntity:
                    GetLegalEntityResponse r2 =
                        this.profileClient.Value.GetLegalEntity(
                            new GetLegalEntityRequest
                            {
                                ProfileId = this.UserData.ActiveProfileId
                            },
                            cancellationToken: Response.ClientDisconnectedToken);

                    LegalProfileModel m2 = new LegalProfileModel()
                    {
                        Editable = new CommonDataModel(
                            UserData.ActiveProfile.ProfileId,
                            UserData.ActiveProfile.TargetGroupId,
                            r2.Email,
                            r2.Phone,
                            r2.Residence,
                            false),
                        Request = new LegalSpecificModel(
                            r2.Name,
                            r2.Identifier)
                    };

                    return PartialView("Partials/_ProfileInfo", m2);
                case TargetGroupId.PublicAdministration:
                case TargetGroupId.SocialOrganization:
                    GetLegalEntityResponse r3 =
                        this.profileClient.Value.GetLegalEntity(
                            new GetLegalEntityRequest
                            {
                                ProfileId = this.UserData.ActiveProfileId
                            },
                            cancellationToken: Response.ClientDisconnectedToken);

                    InstitutionProfileModel m3 = new InstitutionProfileModel()
                    {
                        Editable = new CommonDataModel(
                            UserData.ActiveProfile.ProfileId,
                            UserData.ActiveProfile.TargetGroupId,
                            r3.Email,
                            r3.Phone,
                            r3.Residence,
                            false),
                        Request = new InstitutionSpecificModel(
                            r3.Name,
                            r3.Identifier,
                            !string.IsNullOrEmpty(r3.ParentGuid) ? Guid.Parse(r3.ParentGuid) : (Guid?)null,
                            r3.ParentName ?? string.Empty)
                    };

                    return PartialView("Partials/_ProfileInfo", m3);
            }

            return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.AdministerProfile)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateProfileInfo(CommonDataModel model)
        {
            try
            {
                if (model.ProfileId != this.UserData.ActiveProfileId)
                {
                    throw new Exception("Profile sync failed");
                }

                if (!ModelState.IsValid)
                {
                    ModelState.AddModelError("", ErrorMessages.ErrorInvalidData);

                    return PartialView("Partials/_ProfileEditable", model);
                }

                UpdateResponse resp = await this.profileClient.Value.UpdateAsync(
                    new UpdateRequest
                    {
                        ProfileId = model.ProfileId,
                        Email = model.Email,
                        Phone = model.Phone,
                        Residence = model.Residence,
                        Sync = model.SyncNotificationSettings,
                        ActionLoginId = this.UserData.LoginId,
                        Ip = this.Request.UserHostAddress
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

                if (!resp.IsSuccessful)
                {
                    switch (resp.Error)
                    {
                        case ED.DomainServices.UpdateProfileValidationError.Ok:
                            break;
                        case ED.DomainServices.UpdateProfileValidationError.DuplicateEmail:
                            ModelState.AddModelError(
                                nameof(CommonDataModel.Email),
                                ErrorMessages.EmailAlredyRegistered);
                            break;
                        case ED.DomainServices.UpdateProfileValidationError.Unknown:
                        default:
                            ModelState.AddModelError("", ErrorMessages.ErrorSystemGeneral);
                            break;
                    }
                }
                else
                {
                    ViewBag.ShowSuccessAlert = true;
                }

                return PartialView("Partials/_ProfileEditable", model);
            }
            catch (Exception ex)
            {
                ElmahLogger.Instance.Error(ex, "Can not update profile");

                ModelState.AddModelError("", ErrorMessages.ErrorSystemGeneral);

                return PartialView("Partials/_ProfileEditable", model);
            }
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.AdministerProfile)]
        [HttpPost]
        public async Task<ActionResult> ProfileNamesSync(PersonSpecificModel model)
        {
            try
            {
                CachedLoginProfile activeProfile = this.UserData.ActiveProfile;

                if (activeProfile.TargetGroupId != (int)TargetGroupId.Individual)
                {
                    ModelState.AddModelError(
                        nameof(model.EGN),
                        ErrorMessages.ProfileNamesSyncInvalidProfile);

                    return PartialView("Partials/_ProfilePersonInfo", model);
                }

                GetRegixPersonInfoResponse regixResp =
                    await profileClient.Value.GetRegixPersonInfoAsync(
                        new GetRegixPersonInfoRequest()
                        {
                            Identifier = activeProfile.Identifier
                        });

                if (regixResp == null || regixResp.Result == null)
                {
                    ModelState.AddModelError(
                        nameof(model.EGN),
                        ErrorMessages.ProfileNamesSyncGraoServiceUnavailable);

                    return PartialView("Partials/_ProfilePersonInfo", model);
                }

                if (!regixResp.Result.Success)
                {
                    ModelState.AddModelError(
                        nameof(model.EGN),
                        $"{ErrorMessages.ProfileNamesSyncGraoNoData} : {regixResp.Result.ErrorMessage}");

                    return PartialView("Partials/_ProfilePersonInfo", model);
                }

                _ = await this.profileClient.Value.UpdateIndividualNamesAsync(
                    new UpdateIndividualNamesRequest
                    {
                        ProfileId = this.UserData.ActiveProfileId,
                        FirstName = regixResp.Result.FirstName,
                        MiddleName = regixResp.Result.SurName,
                        LastName = regixResp.Result.FamilyName,
                        ActionLoginId = this.UserData.LoginId,
                        Ip = this.Request.UserHostAddress
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

                this.HttpContext.ClearCachedUserData();

                model.FirstName = regixResp.Result.FirstName;
                model.MiddleName = regixResp.Result.SurName;
                model.LastName = regixResp.Result.FamilyName;

                return PartialView("Partials/_ProfilePersonInfo", model);
            }
            catch (Exception ex)
            {
                ElmahLogger.Instance.Error(
                    ex,
                    $"Error in ProfileNamesSync for profileId: {this.UserData.ActiveProfileId}");

                ModelState.AddModelError(
                    nameof(model.EGN),
                    ErrorMessages.ErrorSystemGeneral);

                return PartialView("Partials/_ProfilePersonInfo", model);
            }
        }

        [HttpGet]
        [BreadCrumb(3, typeof(EDeliveryResources.Common), "TitleProfileAccessHistory", eLeftMenu.Administration)]
        public async Task<ActionResult> History(int page = 1)
        {
            GetHistoryResponse response =
                await profileClient.Value.GetHistoryAsync(
                    new GetHistoryRequest
                    {
                        ProfileId = this.UserData.ActiveProfileId,
                        Offset = (page - 1) * SystemConstants.PageSize,
                        Limit = SystemConstants.PageSize,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            ResourceManager resourceManager =
                new ResourceManager(typeof(ProfilePage));

            List<ProfileHistoryRecord> parsedHistory = response
                .History
                .Select(x => new ProfileHistoryRecord(x, resourceManager))
                .ToList();

            PagedList.PagedListLight<ProfileHistoryRecord> model =
                new PagedList.PagedListLight<ProfileHistoryRecord>(
                    parsedHistory,
                    SystemConstants.PageSize,
                    page,
                    response.Length);

            if (Request.IsAjaxRequest())
            {
                return PartialView("Partials/_History", model);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> ExportHistory()
        {
            GetHistoryResponse response =
                await profileClient.Value.GetHistoryAsync(
                    new GetHistoryRequest
                    {
                        ProfileId = this.UserData.ActiveProfileId,
                        Offset = 0,
                        Limit = SystemConstants.ExportSize,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            ResourceManager resourceManager =
                new ResourceManager(typeof(ProfilePage));

            List<ProfileHistoryRecord> parsedHistory = response
                .History
                .Select(x => new ProfileHistoryRecord(x, resourceManager))
                .ToList();

            return ExportService.ExportHistory(parsedHistory);
        }

        #region Reports

        /// <summary>
        /// Redirect to reports section on another website
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        public async Task<ActionResult> Reports()
        {
            string reportsUrl = ConfigurationManager.AppSettings["ReportsUrl"];

            JWTTokenData data = new JWTTokenData
            {
                Key = ConfigurationManager.AppSettings["ReportsSecretKey"],
                Issuer = ConfigurationManager.AppSettings["ReportsIssuer"],
                Audience = ConfigurationManager.AppSettings["ReportsAudience"],
                Subject = ConfigurationManager.AppSettings["ReportsSubject"],
                ExpSeconds = int.Parse(ConfigurationManager.AppSettings["ReportsTokenExpSeconds"]),
                Jti = Guid.NewGuid()
            };

            string token = JwtService.GetToken(
                this.UserData.ActiveProfile.TargetGroupId,
                this.UserData.ActiveProfile.Identifier,
                data);

            await CreateRegixReportAuditLog(
                DateTime.Now,
                this.UserData.ActiveProfileId,
                data.Jti,
                token);

            if (!Uri.TryCreate(reportsUrl + token, UriKind.Absolute, out Uri reportsFullUrl))
            {
                ElmahLogger.Instance.Error("Error creating reports full URL from: " + reportsUrl + token);

                return new HttpStatusCodeResult(404);
            }

            return Redirect(reportsFullUrl.ToString());
        }

        /// <summary>
        /// Create a regix report audit log
        /// </summary>
        /// <param name="now"></param>
        /// <param name="jti"></param>
        /// <param name="token"></param>
        private async Task CreateRegixReportAuditLog(
            DateTime now,
            int profileId,
            Guid jti,
            string token)
        {
            try
            {
                await this.journalClient.Value.CreateAsync(
                    new ED.DomainServices.Journals.CreateRequest
                    {
                        Token = jti.ToString(),
                        Data = token,
                        LoginId = this.UserData.LoginId,
                        ProfileId = profileId,
                        DateCreated = DateTime.Now.ToTimestamp()
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

                await CreateJWTAuditLog(
                    now,
                    Utils.DbAuditLogs.JWTTokenType.Regex,
                    jti,
                    token);
            }
            catch (Exception ex)
            {
                ElmahLogger.Instance.Error(
                    ex,
                    "Error in eDeliveryService CreateRegixReprotAuditLog!");
            }
        }

        #endregion Reports

        #region Ahu - hora s uvrejdania

        /// <summary>
        /// Redirect to Ahu section on another website
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        public async Task<ActionResult> Ahu()
        {
            string reportsUrl = ConfigurationManager.AppSettings["AhuUrl"];

            JWTTokenData data = new JWTTokenData
            {
                Key = ConfigurationManager.AppSettings["AhuSecretKey"],
                Issuer = ConfigurationManager.AppSettings["AhuIssuer"],
                Audience = ConfigurationManager.AppSettings["AhuAudience"],
                Subject = ConfigurationManager.AppSettings["AhuSubject"],
                ExpSeconds = int.Parse(ConfigurationManager.AppSettings["AhuTokenExpSeconds"]),
                Jti = Guid.NewGuid()
            };

            string token = JwtService.GetToken(
                this.UserData.ActiveProfile.TargetGroupId,
                this.UserData.ActiveProfile.Identifier,
                data);

            await CreateJWTAuditLog(
                DateTime.Now,
                Utils.DbAuditLogs.JWTTokenType.Ahu,
                data.Jti,
                token);

            if (!Uri.TryCreate(reportsUrl + token, UriKind.Absolute, out Uri reportsFullUrl))
            {
                ElmahLogger.Instance.Error("Error creating reports full URL from: " + reportsUrl + token);

                return new HttpStatusCodeResult(404);
            }

            return Redirect(reportsFullUrl.ToString());
        }

        #endregion Ahu

        #region BulSI - blanki

        /// <summary>
        /// Redirect to BulSI section on another website
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        public async Task<ActionResult> BulSI()
        {
            string reportsUrl = ConfigurationManager.AppSettings["BulSIUrl"];

            BulSIJWTTokenData data = new BulSIJWTTokenData
            {
                Key = ConfigurationManager.AppSettings["BulSISecretKey"],
                Issuer = ConfigurationManager.AppSettings["BulSIIssuer"],
                Audience = ConfigurationManager.AppSettings["BulSIAudience"],
                Subject = ConfigurationManager.AppSettings["BulSISubject"],
                ExpSeconds = int.Parse(ConfigurationManager.AppSettings["BulSITokenExpSeconds"]),
                Jti = Guid.NewGuid(),
                Name = Convert.ToBase64String(Encoding.UTF8.GetBytes(UserData.ActiveProfile.ProfileName)),
                Phone = UserData.ActiveProfile.Phone,
                Email = UserData.ActiveProfile.Email
            };

            string token = JwtService.GetToken(
                this.UserData.ActiveProfile.TargetGroupId,
                this.UserData.ActiveProfile.Identifier,
                data);

            await CreateJWTAuditLog(
                DateTime.Now,
                Utils.DbAuditLogs.JWTTokenType.BulSI,
                data.Jti,
                token);

            if (!Uri.TryCreate(reportsUrl + token, UriKind.Absolute, out Uri reportsFullUrl))
            {
                ElmahLogger.Instance.Error("Error creating reports full URL from: " + reportsUrl + token);

                return new HttpStatusCodeResult(404);
            }

            return Redirect(reportsFullUrl.ToString());
        }

        #endregion BulSI - blanki

        #region pay.egov.bg

        /// <summary>
        /// Redirect to pay.egov.bg
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Pay()
        {
            string payUrl = ConfigurationManager.AppSettings["PayUrl"];

            PaymentsJWTTokenData data = new PaymentsJWTTokenData
            {
                Key = ConfigurationManager.AppSettings["PaySecretKey"],
                Issuer = ConfigurationManager.AppSettings["PayIssuer"],
                Audience = ConfigurationManager.AppSettings["PayAudience"],
                Subject = ConfigurationManager.AppSettings["PaySubject"],
                ExpSeconds = int.Parse(ConfigurationManager.AppSettings["PayTokenExpSeconds"]),
                Name = Convert.ToBase64String(Encoding.UTF8.GetBytes(UserData.ActiveProfile.ProfileName)),
                Jti = Guid.NewGuid()
            };

            string token = JwtService.GetToken(
                this.UserData.ActiveProfile.TargetGroupId,
                this.UserData.ActiveProfile.Identifier,
                data);

            //log the token
            await CreateJWTAuditLog(
                DateTime.Now,
                Utils.DbAuditLogs.JWTTokenType.Payments,
                data.Jti,
                token);

            if (!Uri.TryCreate(payUrl + token, UriKind.Absolute, out Uri payFullUrl))
            {
                ElmahLogger.Instance.Error("Error creating payment full URL from: " + payUrl + token);

                return new HttpStatusCodeResult(404);
            }

            return Redirect(payFullUrl.ToString());
        }

        #endregion pay.egov.bg

        /// <summary>
        /// Create a regix report audit log
        /// </summary>
        /// <param name="now"></param>
        /// <param name="jti"></param>
        /// <param name="token"></param>
        private async Task CreateJWTAuditLog(
            DateTime now,
            Utils.DbAuditLogs.JWTTokenType tokenType,
            Guid jti,
            string token)
        {
            try
            {
                using (Utils.DbAuditLogs.EDeliveryLoggingDB loggingdb =
                    new Utils.DbAuditLogs.EDeliveryLoggingDB(
                        WebConfigurationManager.ConnectionStrings["LoggingDB"].ConnectionString))
                {
                    Utils.DbAuditLogs.JWTTokenAuditLog log =
                        new Utils.DbAuditLogs.JWTTokenAuditLog()
                        {
                            DateCreated = now,
                            LoginId = UserData?.LoginId,
                            ProfileElectronicSubjectId = UserData?.ActiveProfile?.ProfileGuid,
                            ProfileId = UserData?.ActiveProfileId,
                            Token = token,
                            TokenJti = jti,
                            TokenType = (int)tokenType,
                            Username = User?.Identity?.Name
                        };

                    loggingdb.JWTTokenAuditLogs.Add(log);

                    await loggingdb.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                ElmahLogger.Instance.Error(
                    ex,
                    "Error in CreateJWTAuditLog");
            }
        }
    }
}
