namespace ED.Domain
{
    public class DateTimeComponent : ValueComponent
    {
        public DateTimeComponent()
        {
            this.Type = ComponentType.datetime;
        }

        public bool UseNowAsDefaultValue { get; set; }
    }
}
