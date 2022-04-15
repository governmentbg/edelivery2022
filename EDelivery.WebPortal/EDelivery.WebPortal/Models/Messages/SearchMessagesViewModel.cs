
using System;

namespace EDelivery.WebPortal.Models
{
    public class SearchMessagesViewModel
    {
        public string Title { get; set; }

        public string Subject { get; set; }

        public string Orn { get; set; }

        public string ReferencedOrn { get; set; }

        public string FromDateAsString { get; set; }

        public string ToDateAsString { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public bool HasFilter
        {
            get
            {
                return !string.IsNullOrEmpty(this.Title)
                    || !string.IsNullOrEmpty(this.Subject)
                    || this.FromDate.HasValue
                    || this.ToDate.HasValue;
            }
        }
    }
}
