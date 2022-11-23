namespace ED.AdminPanel.Blazor.Pages.Templates.Components.Models
{
    public class MarkdownComponent : ValueComponent
    {
        public MarkdownComponent()
        {
            this.Type = ComponentType.markdown;
        }

        public string PdfValue { get; set; }
    }
}
