namespace EDelivery.WebPortal.Models
{
    public class StringViewModel
    {
        public StringViewModel() { }

        public StringViewModel(string value) => Value = value;

        public string Value { get; set; }
    }
}