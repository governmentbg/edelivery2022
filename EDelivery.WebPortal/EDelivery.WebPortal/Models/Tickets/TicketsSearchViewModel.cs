using System.Collections.Generic;
using System.Web.Mvc;

using ED.DomainServices;

namespace EDelivery.WebPortal.Models.Tickets
{
    public class TicketsSearchViewModel
    {
        public TicketsSearchViewModel()
        {
        }

        public TicketsSearchViewModel(
            string from,
            string to,
            int? status)
        {
            this.From = from;
            this.To = to;
            this.Status = status;
        }

        public string From { get; set; }

        public string To { get; set; }

        public bool HasFilter
        {
            get
            {
                return !string.IsNullOrEmpty(this.From)
                    || !string.IsNullOrEmpty(this.To)
                    || this.Status.HasValue;
            }
        }

        public int? Status { get; set; }

        public IEnumerable<SelectListItem> Statuses =>
            new List<SelectListItem>
            {
                new SelectListItem() {Value = null, Text = string.Empty, Selected = true},
                new SelectListItem() {Value = ((int)TicketStatusStatus.NonServed).ToString(), Text = "Невръчен", Selected = false},
                new SelectListItem() {Value = ((int)TicketStatusStatus.InternallyServed).ToString(), Text = "Връчен през ССЕВ", Selected = false},
                new SelectListItem() {Value = ((int)TicketStatusStatus.ExternallyServed).ToString(), Text = "Връчен извън ССЕВ", Selected = false},
                new SelectListItem() {Value = ((int)TicketStatusStatus.Annulled).ToString(), Text = "Анулиран", Selected = false},
            };
    }
}
