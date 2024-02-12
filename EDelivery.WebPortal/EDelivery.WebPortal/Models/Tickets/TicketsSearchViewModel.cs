namespace EDelivery.WebPortal.Models.Tickets
{
    public class TicketsSearchViewModel
    {
        public TicketsSearchViewModel()
        {
        }

        public TicketsSearchViewModel(
            string from,
            string to)
        {
            this.From = from;
            this.To = to;
        }

        public string From { get; set; }

        public string To { get; set; }

        public bool HasFilter
        {
            get
            {
                return !string.IsNullOrEmpty(this.From)
                    || !string.IsNullOrEmpty(this.To);
            }
        }
    }
}
