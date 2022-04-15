namespace ED.AdminPanel.Blazor.Pages.Templates.Components.Models
{
    public class TextFieldComponent : ValueComponent
    {
        public TextFieldComponent()
        {
            this.Type = ComponentType.textfield;
        }

        public string Placeholder { get; set; }
    }
}
