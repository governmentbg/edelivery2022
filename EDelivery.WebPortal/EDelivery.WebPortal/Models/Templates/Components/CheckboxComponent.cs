namespace EDelivery.WebPortal.Models.Templates.Components
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