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

namespace ED.AdminPanel.Blazor.Pages.Registrations
{
    public class CreateModel
    {
        [Display(
            Name = nameof(IndexResources.BlobId),
            ResourceType = typeof(IndexResources))]
        public BlobValue BlobValue { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(IndexResources.Name),
            ResourceType = typeof(IndexResources))]
        public string Name { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(IndexResources.Identifier),
            ResourceType = typeof(IndexResources))]
        public string Identifier { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(IndexResources.Phone),
            ResourceType = typeof(IndexResources))]
        [RegularExpression(
            Constants.PhoneRegex,
            ErrorMessageResourceName = nameof(ErrorMessages.ErrorInvalidPhone),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        public string Phone { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(IndexResources.Email),
            ResourceType = typeof(IndexResources))]
        public string Email { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(IndexResources.Residence),
            ResourceType = typeof(IndexResources))]
        public string Residence { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(IndexResources.TargetGroup),
            ResourceType = typeof(IndexResources))]
        public string TargetGroupId { get; set; }
    }

    public partial class Index
    {
        [Inject] private Nomenclature.NomenclatureClient NomenclatureClient { get; set; }

        [Inject] private Admin.AdminClient AdminClient { get; set; }

        [Inject] private IStringLocalizer<IndexResources> Localizer { get; set; }

        [Inject] private NavigationManager NavigationManager { get; set; }

        [Inject] private AuthenticationStateHelper AuthenticationStateHelper { get; set; }

        private CreateModel model;
        private Select2Option[] targetGroups;
        private ServerSideValidator serverSideValidator;

        protected override async Task OnInitializedAsync()
        {
            this.model = new();

            GetActiveEntityNomResponse targetGroupsResponse =
               await this.NomenclatureClient.GetTargetGroupsAsync(
                   new GetNomRequest
                   {
                       Term = string.Empty,
                   });

            this.targetGroups = targetGroupsResponse
                .Result
                .Where(n => n.Id != Constants.IndividualTargetGroupId
                    && n.IsActive)
                .Select(n =>
                    new Select2Option
                    {
                        Id = n.Id.ToString(),
                        Text = n.Name,
                    })
                .ToArray();
        }

        private async Task ChangedBlobValue(BlobValue blobValue)
        {
            this.model.BlobValue = blobValue;

            if (blobValue != null)
            {
                ParseRegistrationDocumentResponse response =
                    await this.AdminClient.ParseRegistrationDocumentAsync(
                        new ParseRegistrationDocumentRequest
                        {
                            BlobId = blobValue.BlobId
                        });

                if (response.IsSuccessful)
                {
                    this.model.Name = response.Result.Name;
                    this.model.Identifier = response.Result.Identifier;
                    this.model.Phone = response.Result.Phone;
                    this.model.Email = response.Result.Email;
                    this.model.Residence = response.Result.Residence;
                }
                else
                {
                    this.serverSideValidator.DisplayErrors(
                        new Dictionary<string, List<string>>
                        {
                            {
                                nameof(CreateModel.BlobValue),
                                new List<string> { IndexResources.InvalidBlob }
                            }
                        });
                }
            }
        }

        private async Task SaveAsync()
        {
            int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();

            RegisterProfileResponse response =
                await this.AdminClient.RegisterProfileAsync(
                    new RegisterProfileRequest
                    {
                        Name = this.model.Name,
                        Identifier = this.model.Identifier,
                        Phone = this.model.Phone,
                        Email = this.model.Email,
                        Residence = this.model.Residence,
                        TargetGroupId = int.Parse(this.model.TargetGroupId),
                        BlobId = this.model.BlobValue?.BlobId,
                        AdminUserId = currentUserId
                    });

            if (response.IsSuccessful)
            {
                this.NavigationManager.NavigateTo($"profiles/{response.ProfileId!}");
            }
            else
            {
                this.serverSideValidator.ClearErrors();

                this.serverSideValidator.DisplayErrors(
                    new Dictionary<string, List<string>>
                    {
                        {
                            nameof(CreateModel.Name),
                            new List<string> { response.Error }
                        }
                    });
            }
        }
    }
}
