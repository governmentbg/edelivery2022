using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using ED.AdminPanel.Blazor.Shared.Fields;
using ED.DomainServices.Admin;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Primitives;

namespace ED.AdminPanel.Blazor.Pages.Templates
{
    public class SearchTemplateModel
    {
        [Display(
            Name = nameof(IndexTemplates.FormTerm),
            ResourceType = typeof(IndexTemplates))]
        public string TemplateStatus { get; set; }

        [Display(
            Name = nameof(IndexTemplates.FormTemplateStatus),
            ResourceType = typeof(IndexTemplates))]
        public string Term { get; set; }
    }

    public partial class Index
    {
        [Inject] private IStringLocalizer<IndexTemplates> Localizer { get; set; }

        [Inject] private Admin.AdminClient AdminClient { get; set; }

        private EditContext editContext;

        private SearchTemplateModel model = new();
        private GetTemplateListResponse templates;
        private Select2Option[] templateStatuses;

        protected override void ExtractQueryStringParams()
        {
            base.ExtractQueryStringParams();

            this.model.TemplateStatus = this.NavigationManager.GetCurrentQueryItem<string>("templateStatus");
            this.model.Term = this.NavigationManager.GetCurrentQueryItem<string>("term");
        }

        protected override void OnInitialized()
        {
            this.templateStatuses = new Select2Option[]
            {
                new Select2Option { Text = this.Localizer["OptionNone"], Id = ((int)DomainServices.TemplateStatus.None).ToString() },
                new Select2Option { Text = this.Localizer["OptionPublished"], Id = ((int)DomainServices.TemplateStatus.Published).ToString() },
                new Select2Option { Text = this.Localizer["OptionDraft"], Id = ((int)DomainServices.TemplateStatus.Draft).ToString() },
                new Select2Option { Text = this.Localizer["OptionArchived"], Id = ((int)DomainServices.TemplateStatus.Archived).ToString() },
            };

            this.model = new();

            this.editContext = new EditContext(this.model);

            base.OnInitialized();
        }

        protected override async Task LoadDataAsync(CancellationToken ct)
        {
            this.templates =
                await this.AdminClient.GetTemplateListAsync(
                    new GetTemplateListRequest
                    {
                        Term = this.model.Term,
                        TemplateStatus = this.model.TemplateStatus == null
                            ? DomainServices.TemplateStatus.None
                            : Enum.Parse<DomainServices.TemplateStatus>(this.model.TemplateStatus),
                        Offset = base.Offset,
                        Limit = base.Limit,
                    },
                    cancellationToken: ct);
        }

        private void Search()
        {
            this.NavigationManager.NavigateToSameWithQuery(
                new Dictionary<string, StringValues>
                {
                    { "page", 1.ToString() },
                    { "templateStatus", this.model.TemplateStatus?.ToString() ?? ((int)DomainServices.TemplateStatus.None).ToString() },
                    { "term", this.model.Term },
                });
        }
    }
}
