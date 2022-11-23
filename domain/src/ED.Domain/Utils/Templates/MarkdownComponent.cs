namespace ED.Domain
{
    public class MarkdownComponent : ValueComponent
    {
        public MarkdownComponent()
        {
            this.Type = ComponentType.markdown;
        }

        public string? PdfValue { get; set; }
    }
}
