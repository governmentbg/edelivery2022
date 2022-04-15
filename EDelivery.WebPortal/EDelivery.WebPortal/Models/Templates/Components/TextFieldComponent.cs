namespace EDelivery.WebPortal.Models.Templates.Components
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