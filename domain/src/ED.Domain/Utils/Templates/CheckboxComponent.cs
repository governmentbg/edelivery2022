namespace ED.Domain
{
    public class CheckboxComponent : BaseComponent
    {
        public CheckboxComponent()
        {
            this.Type = ComponentType.checkbox;
        }

        public bool Value { get; set; }
    }
}
