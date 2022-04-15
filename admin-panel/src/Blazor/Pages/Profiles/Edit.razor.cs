using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ED.AdminPanel.Blazor.Shared;
using ED.AdminPanel.Blazor.Shared.Fields;
using ED.AdminPanel.Resources;
using ED.DomainServices.Admin;
using ED.DomainServices.Nomenclatures;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace ED.AdminPanel.Blazor.Pages.Profiles
{
    public class EditModel : IValidatableObject
    {
        public bool IsIndividual { get; set; }

        [Display(
            Name = nameof(EditResources.FirstName),
            ResourceType = typeof(EditResources))]
        public string FirstName { get; set; }

        [Display(
            Name = nameof(EditResources.MiddleName),
            ResourceType = typeof(EditResources))]
        public string MiddleName { get; set; }

        [Display(
            Name = nameof(EditResources.LastName),
            ResourceType = typeof(EditResources))]
        public string LastName { get; set; }

        [Display(
            Name = nameof(EditResources.IndividualIdentifier),
            ResourceType = typeof(EditResources))]
        public string IndividualIdentifier { get; set; }

        [Display(
            Name = nameof(EditResources.Name),
            ResourceType = typeof(EditResources))]
        public string Name { get; set; }

        [Display(
            Name = nameof(EditResources.LegalEntityIdentifier),
            ResourceType = typeof(EditResources))]
        public string LegalEntityIdentifier { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(EditResources.Phone),
            ResourceType = typeof(EditResources))]
        public string Phone { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(EditResources.EmailAddress),
            ResourceType = typeof(EditResources))]
        public string EmailAddress { get; set; }

        [Display(
            Name = nameof(EditResources.AddressCountry),
            ResourceType = typeof(EditResources))]
        public string AddressCountryCode { get; set; }

        [Display(
            Name = nameof(EditResources.AddressState),
            ResourceType = typeof(EditResources))]
        public string AddressState { get; set; }

        [Display(
            Name = nameof(EditResources.AddressCity),
            ResourceType = typeof(EditResources))]
        public string AddressCity { get; set; }

        [Display(
            Name = nameof(EditResources.AddressResidence),
            ResourceType = typeof(EditResources))]
        public string AddressResidence { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(EditResources.TargetGroup),
            ResourceType = typeof(EditResources))]
        public string TargetGroupId { get; set; }

        [Display(
            Name = nameof(EditResources.FormEnableMessagesWithCode),
            ResourceType = typeof(EditResources))]
        public string EnableMessagesWithCodeAsString { get; set; }

        public bool? EnableMessagesWithCode { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (this.IsIndividual)
            {
                foreach (var vr in this.RequiredValidation(m => m.FirstName))
                    yield return vr;
                foreach (var vr in this.RequiredValidation(m => m.MiddleName))
                    yield return vr;
                foreach (var vr in this.RequiredValidation(m => m.LastName))
                    yield return vr;
                foreach (var vr in this.RequiredValidation(m => m.IndividualIdentifier))
                    yield return vr;
            }
            else
            {
                foreach (var vr in this.RequiredValidation(m => m.Name))
                    yield return vr;
                foreach (var vr in this.RequiredValidation(m => m.LegalEntityIdentifier))
                    yield return vr;
            }
        }
    }

    public partial class Edit
    {
        [Parameter] public int ProfileId { get; set; }

        [CascadingParameter] private ConnectionInfo ConnectionInfo { get; set; }

        [Inject] private Nomenclature.NomenclatureClient NomenclatureClient { get; set; }

        [Inject] private Admin.AdminClient AdminClient { get; set; }

        [Inject] private IStringLocalizer<EditResources> Localizer { get; set; }

        [Inject] private NavigationManager NavigationManager { get; set; }

        [Inject] private AuthenticationStateHelper AuthenticationStateHelper { get; set; }

        private EditModel model;
        private Select2Option[] enableMessagesWithCode;
        private Select2Option[] countries;
        private Select2Option[] targetGroups;

        private ServerSideValidator serverSideValidator;

        protected override async Task OnInitializedAsync()
        {
            int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();
            GetProfileDataResponse data =
                await this.AdminClient.GetProfileDataAsync(
                    new GetProfileDataRequest
                    {
                        ProfileId = this.ProfileId,
                        AdminUserId = currentUserId,
                    });

            bool isIndividual = data.DataCase == GetProfileDataResponse.DataOneofCase.IndividualData;

            this.model = new()
            {
                IsIndividual = isIndividual,
                FirstName = data.IndividualData?.FirstName,
                MiddleName = data.IndividualData?.MiddleName,
                LastName = data.IndividualData?.LastName,
                IndividualIdentifier = isIndividual ? data.Identifier : null,
                Name = data.LegalEntityData?.Name,
                LegalEntityIdentifier = !isIndividual ? data.Identifier : null,
                Phone = data.Phone,
                EmailAddress = data.EmailAddress,
                AddressCountryCode = data.AddressCountryCode,
                AddressState = data.AddressState,
                AddressCity = data.AddressCity,
                AddressResidence = data.AddressResidence,
                TargetGroupId = data.TargetGroupId.ToString(),
                EnableMessagesWithCodeAsString = data.EnableMessagesWithCode?.ToString() ?? string.Empty,
                EnableMessagesWithCode = data.EnableMessagesWithCode
            };

            this.enableMessagesWithCode = new Select2Option[]
            {
                new Select2Option { Text = this.Localizer["FormEnableMessagesWithCodeValueNull"], Id = "null" },
                new Select2Option { Text = this.Localizer["FormEnableMessagesWithCodeValueTrue"], Id = true.ToString() },
                new Select2Option { Text = this.Localizer["FormEnableMessagesWithCodeValueFalse"], Id = false.ToString() },
            };

            GetEntityCodeNomResponse countriesResponse =
                await this.NomenclatureClient.GetCountriesAsync(
                    new GetNomRequest
                    {
                        Term = string.Empty
                    });

            this.countries =
                countriesResponse.Result
                .Select(n =>
                    new Select2Option
                    {
                        Id = n.Id,
                        Text = n.Name,
                    })
                .ToArray();

            GetActiveEntityNomResponse targetGroupsResponse =
               await this.NomenclatureClient.GetTargetGroupsAsync(
                   new GetNomRequest
                   {
                       Term = string.Empty,
                   });

            if (data.TargetGroupId == Constants.IndividualTargetGroupId)
            {
                this.targetGroups = targetGroupsResponse
                    .Result
                    .Where(n => n.Id == Constants.IndividualTargetGroupId)
                    .Select(n =>
                        new Select2Option
                        {
                            Id = n.Id.ToString(),
                            Text = n.Name,
                        })
                    .ToArray();
            }
            else
            {
                this.targetGroups = targetGroupsResponse
                    .Result
                    .Where(n => n.Id != Constants.IndividualTargetGroupId)
                    .Where(n => !data.IsActivated || n.IsActive)
                    .Select(n =>
                        new Select2Option
                        {
                            Id = n.Id.ToString(),
                            Text = n.Name,
                        })
                    .ToArray();
            }
        }

        private async Task SaveAsync()
        {
            int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            bool? enableMessagesWithCode =
                bool.TryParse(this.model.EnableMessagesWithCodeAsString, out bool parsedEnableMessagesWithCode)
                    ? parsedEnableMessagesWithCode
                    : null;

            UpdateProfileDataRequest request = new()
            {
                ProfileId = this.ProfileId,
                AdminUserId = currentUserId,
                Phone = this.model.Phone,
                EmailAddress = this.model.EmailAddress,
                AddressCountryCode = this.model.AddressCountryCode,
                AddressState = this.model.AddressState,
                AddressCity = this.model.AddressCity,
                AddressResidence = this.model.AddressResidence,
                TargetGroupId = int.Parse(this.model.TargetGroupId),
                EnableMessagesWithCode = enableMessagesWithCode,
                Ip = this.ConnectionInfo.RemoteIpAddress,
            };

            if (this.model.IsIndividual)
            {
                request.IndividualData = new()
                {
                    FirstName = this.model.FirstName,
                    MiddleName = this.model.MiddleName,
                    LastName = this.model.LastName,
                };
                request.Identifier = this.model.IndividualIdentifier;
            }
            else
            {
                request.LegalEntityData = new()
                {
                    Name = this.model.Name,
                };
                request.Identifier = this.model.LegalEntityIdentifier;
            }

            UpdateProfileDataResponse resp =
                await this.AdminClient.UpdateProfileDataAsync(request);

            if (resp.IsSuccessful)
            {
                this.NavigationManager.NavigateTo($"profiles/{this.ProfileId}");
            }
            else
            {
                this.serverSideValidator.ClearErrors();

                if (this.model.IsIndividual)
                {
                    this.serverSideValidator.DisplayErrors(
                        new Dictionary<string, List<string>>
                        {
                            {
                                nameof(EditModel.IndividualIdentifier),
                                new List<string> { resp.Error }
                            }
                        });
                }
                else
                {
                    this.serverSideValidator.DisplayErrors(
                        new Dictionary<string, List<string>>
                        {
                            {
                                nameof(EditModel.LegalEntityIdentifier),
                                new List<string> { resp.Error }
                            }
                        });
                }
            }
        }
    }
}
