using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Blazored.Modal;
using Blazored.Modal.Services;
using ED.AdminPanel.Blazor.Pages.Templates.Components.Create;
using ED.AdminPanel.Blazor.Pages.Templates.Components.Models;
using ED.AdminPanel.Blazor.Shared.Fields;
using ED.AdminPanel.Resources;
using ED.DomainServices.Admin;
using ED.DomainServices.Nomenclatures;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace ED.AdminPanel.Blazor.Pages.Templates
{
    public class CreateEditModel
    {
        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(CreateEditResources.Name),
            ResourceType = typeof(CreateEditResources))]
        public string Name { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(CreateEditResources.IdentityNumber),
            ResourceType = typeof(CreateEditResources))]
        public string IdentityNumber { get; set; }

        public bool IsSystemTemplate { get; set; }

        public string ResponseTemplateId { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(CreateEditResources.ReadLoginSecurityLevelId),
            ResourceType = typeof(CreateEditResources))]
        public string ReadLoginSecurityLevelId { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(CreateEditResources.WriteLoginSecurityLevelId),
            ResourceType = typeof(CreateEditResources))]
        public string WriteLoginSecurityLevelId { get; set; }

        [Required(
            ErrorMessageResourceName = nameof(ErrorMessages.Required),
            ErrorMessageResourceType = typeof(ErrorMessages))]
        [Display(
            Name = nameof(CreateEditResources.BlobId),
            ResourceType = typeof(CreateEditResources))]
        public BlobValue BlobValue { get; set; }

        [Display(
            Name = nameof(CreateEditResources.FormSenderDocumentField),
            ResourceType = typeof(CreateEditResources))]
        public string SenderDocumentField { get; set; }

        [Display(
            Name = nameof(CreateEditResources.FormRecipientDocumentField),
            ResourceType = typeof(CreateEditResources))]
        public string RecipientDocumentField { get; set; }

        [Display(
            Name = nameof(CreateEditResources.FormSubjectDocumentField),
            ResourceType = typeof(CreateEditResources))]
        public string SubjectDocumentField { get; set; }

        [Display(
            Name = nameof(CreateEditResources.FormDateSentDocumentField),
            ResourceType = typeof(CreateEditResources))]
        public string DateSentDocumentField { get; set; }

        [Display(
            Name = nameof(CreateEditResources.FormDateReceivedDocumentField),
            ResourceType = typeof(CreateEditResources))]
        public string DateReceivedDocumentField { get; set; }

        [Display(
            Name = nameof(CreateEditResources.FormSenderSignatureDocumentField),
            ResourceType = typeof(CreateEditResources))]
        public string SenderSignatureDocumentField { get; set; }

        [Display(
            Name = nameof(CreateEditResources.FormRecipientSignatureDocumentField),
            ResourceType = typeof(CreateEditResources))]
        public string RecipientSignatureDocumentField { get; set; }

#pragma warning disable CA2227 // Collection properties should be read only
        public IList<BaseComponent> Content { get; set; } = new List<BaseComponent>();
#pragma warning restore CA2227 // Collection properties should be read only
    }

    public partial class CreateEdit
    {
        [Parameter] public int? CopyFromTemplateId { get; set; }

        [Parameter] public int? EditTemplateId { get; set; }

        [CascadingParameter] private IModalService Modal { get; set; }

        [Inject] private Nomenclature.NomenclatureClient NomenclatureClient { get; set; }

        [Inject] private Admin.AdminClient AdminClient { get; set; }

        [Inject] private IStringLocalizer<CreateEditResources> Localizer { get; set; }

        [Inject] private NavigationManager NavigationManager { get; set; }

        [Inject] private AuthenticationStateHelper AuthenticationStateHelper { get; set; }

        private CreateEditModel model;
        private Select2Option[] loginSecurityLevels;
        private Select2Option[] responseTemplates;

        protected override async Task OnInitializedAsync()
        {
            this.model = new();

            if (this.CopyFromTemplateId != null)
            {
                GetTemplateResponse templateResponse =
                    await this.AdminClient.GetTemplateAsync(
                        new GetTemplateRequest
                        {
                            TemplateId = this.CopyFromTemplateId.Value
                        });

                GetTemplateResponse.Types.TemplateMessage template = templateResponse.Template;

                this.model.Content =
                    SerializationHelper.DeserializeModel(template.Content);
                this.model.ReadLoginSecurityLevelId =
                    template.ReadLoginSecurityLevelId.ToString();
                this.model.WriteLoginSecurityLevelId =
                    template.WriteLoginSecurityLevelId.ToString();
                this.model.SenderDocumentField = template.SenderDocumentField;
                this.model.RecipientDocumentField = template.RecipientDocumentField;
                this.model.SubjectDocumentField = template.SubjectDocumentField;
                this.model.DateSentDocumentField = template.DateSentDocumentField;
                this.model.DateReceivedDocumentField = template.DateReceivedDocumentField;
                this.model.SenderSignatureDocumentField = template.SenderSignatureDocumentField;
                this.model.RecipientSignatureDocumentField = template.RecipientSignatureDocumentField;
            }

            if (this.EditTemplateId != null)
            {
                GetTemplateResponse templateResponse =
                    await this.AdminClient.GetTemplateAsync(
                        new GetTemplateRequest
                        {
                            TemplateId = this.EditTemplateId.Value
                        });

                GetTemplateResponse.Types.TemplateMessage template = templateResponse.Template;

                this.model.Name = template.Name;
                this.model.IdentityNumber = template.IdentityNumber;
                this.model.Content =
                    SerializationHelper.DeserializeModel(template.Content);
                this.model.IsSystemTemplate = template.IsSystemTemplate;
                this.model.ResponseTemplateId =
                    template.ResponseTemplateId.ToString();
                this.model.ReadLoginSecurityLevelId =
                    template.ReadLoginSecurityLevelId.ToString();
                this.model.WriteLoginSecurityLevelId =
                    template.WriteLoginSecurityLevelId.ToString();
                if (template.BlobId.HasValue)
                {
                    this.model.BlobValue = new(
                        template.BlobName,
                        template.BlobId.Value);
                }
                this.model.SenderDocumentField = template.SenderDocumentField;
                this.model.RecipientDocumentField = template.RecipientDocumentField;
                this.model.SubjectDocumentField = template.SubjectDocumentField;
                this.model.DateSentDocumentField = template.DateSentDocumentField;
                this.model.DateReceivedDocumentField = template.DateReceivedDocumentField;
                this.model.SenderSignatureDocumentField = template.SenderSignatureDocumentField;
                this.model.RecipientSignatureDocumentField = template.RecipientSignatureDocumentField;
            }

            var loginSecurityLevelsResponse =
                await this.NomenclatureClient.GetLoginSecurityLevelAsync(
                    new GetNomRequest
                    {
                        Term = string.Empty,
                    });

            this.loginSecurityLevels =
                loginSecurityLevelsResponse.Result
                .Select(n =>
                    new Select2Option
                    {
                        Id = n.Id.ToString(),
                        Text = n.Name,
                    })
                .ToArray();

            var responseTemplatesResponse =
                await this.AdminClient.GetTemplateListAsync(
                    new GetTemplateListRequest
                    {
                        Offset = 0,
                        Limit = 10000
                    });

            this.responseTemplates =
                responseTemplatesResponse.Result
                .Select(t =>
                    new Select2Option
                    {
                        Id = t.TemplateId.ToString(),
                        Text = t.Name,
                    })
                .ToArray();
        }

        private async Task AddFieldAsync(ComponentType componentType)
        {
            var componentModal = this.Modal.Show(this.GetFormModalType(componentType));
            var result = await componentModal.Result;

            if (!result.Cancelled)
            {
                this.model.Content.Add((BaseComponent)result.Data);
            }
        }

        private async Task EditFieldAsync(BaseComponent fieldModel)
        {
            var parameters = new ModalParameters();
            parameters.Add("Model", fieldModel);

            var componentModal = this.Modal.Show(
                this.GetFormModalType(fieldModel.Type),
                string.Empty,
                parameters);
            var result = await componentModal.Result;

            if (!result.Cancelled)
            {
                int fieldIndex = this.model.Content.IndexOf(fieldModel);
                this.model.Content[fieldIndex] = (BaseComponent)result.Data;
            }
        }

        private void RemoveField(BaseComponent fieldModel)
        {
            this.model.Content.Remove(fieldModel);
        }

        private Type GetFormModalType(ComponentType componentType)
            => componentType switch
            {
                ComponentType.checkbox => typeof(CheckboxForm),
                ComponentType.datetime => typeof(DateTimeForm),
                ComponentType.file => typeof(FileForm),
                ComponentType.hidden => typeof(HiddenForm),
                ComponentType.select => typeof(SelectForm),
                ComponentType.textfield => typeof(TextFieldForm),
                ComponentType.textarea => typeof(TextAreaForm),
                _ => throw new Exception($"Unknown {nameof(componentType)} {componentType}"),
            };

        private async Task SaveAsync()
        {
            var name = this.model.Name;
            var identityNumber = this.model.IdentityNumber;
            var content = SerializationHelper.SerializeModel(this.model.Content);
            var isSystemTemplate = this.model.IsSystemTemplate;
            var responseTemplateId =
                string.IsNullOrEmpty(this.model.ResponseTemplateId)
                    ? (int?)null
                    : int.Parse(this.model.ResponseTemplateId);
            var readLoginSecurityLevelId = int.Parse(this.model.ReadLoginSecurityLevelId);
            var writeLoginSecurityLevelId = int.Parse(this.model.WriteLoginSecurityLevelId);
            var blobId = this.model.BlobValue.BlobId;
            var senderDocumentField = this.model.SenderDocumentField;
            var recipientDocumentField = this.model.RecipientDocumentField;
            var subjectDocumentField = this.model.SubjectDocumentField;
            var dateSentDocumentField = this.model.DateSentDocumentField;
            var dateReceivedDocumentField = this.model.DateReceivedDocumentField;
            var senderSignatureDocumentField = this.model.SenderSignatureDocumentField;
            var recipientSignatureDocumentField = this.model.RecipientSignatureDocumentField;

            if (this.EditTemplateId == null)
            {
                int currentUserId = await this.AuthenticationStateHelper.GetAuthenticatedUserId();

                CreateTemplateResponse resp =
                    await this.AdminClient.CreateTemplateAsync(
                        new CreateTemplateRequest
                        {
                            Name = name,
                            IdentityNumber = identityNumber,
                            Content = content,
                            IsSystemTemplate = isSystemTemplate,
                            ResponseTemplateId = responseTemplateId,
                            ReadLoginSecurityLevelId = readLoginSecurityLevelId,
                            WriteLoginSecurityLevelId = writeLoginSecurityLevelId,
                            CreatedByAdminUserId = currentUserId,
                            BlobId = blobId,
                            SenderDocumentField = senderDocumentField,
                            RecipientDocumentField = recipientDocumentField,
                            SubjectDocumentField = subjectDocumentField,
                            DateSentDocumentField = dateSentDocumentField,
                            DateReceivedDocumentField = dateReceivedDocumentField,
                            SenderSignatureDocumentField = senderSignatureDocumentField,
                            RecipientSignatureDocumentField = recipientSignatureDocumentField,
                        });

                this.NavigationManager.NavigateTo($"templates/{resp.TemplateId}");
            }
            else
            {
                await this.AdminClient.EditTemplateAsync(
                    new EditTemplateRequest
                    {
                        TemplateId = this.EditTemplateId.Value,
                        Name = name,
                        IdentityNumber = identityNumber,
                        Content = content,
                        IsSystemTemplate = isSystemTemplate,
                        ResponseTemplateId = responseTemplateId,
                        ReadLoginSecurityLevelId = readLoginSecurityLevelId,
                        WriteLoginSecurityLevelId = writeLoginSecurityLevelId,
                        BlobId = blobId,
                        SenderDocumentField = senderDocumentField,
                        RecipientDocumentField = recipientDocumentField,
                        SubjectDocumentField = subjectDocumentField,
                        DateSentDocumentField = dateSentDocumentField,
                        DateReceivedDocumentField = dateReceivedDocumentField,
                        SenderSignatureDocumentField = senderSignatureDocumentField,
                        RecipientSignatureDocumentField = recipientSignatureDocumentField,
                    });

                this.NavigationManager.NavigateTo($"templates/{this.EditTemplateId.Value}");
            }
        }
    }
}
