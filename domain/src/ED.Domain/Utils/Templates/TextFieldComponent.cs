namespace ED.Domain
{
    public class TextFieldComponent : ValueComponent
    {
        public TextFieldComponent()
        {
            this.Type = ComponentType.textfield;
        }

        public string? Placeholder { get; set; }
    }
}
