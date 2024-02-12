using ED.DomainServices;

using System;

namespace EDelivery.WebPortal.Models.Tickets
{
    public class TicketsInboxViewModel
    {
        public TicketsInboxViewModel() { }

        public TicketsSearchViewModel SearchFilter { get; set; }

        public PagedList.PagedListLight<TicketsInboxViewModelItem> Tickets { get; set; }
    }

    public class TicketsInboxViewModelItem
    {
        public TicketsInboxViewModelItem(ED.DomainServices.Tickets.InboxResponse.Types.Ticket ticket)
        { 
            this.MessageId = ticket.MessageId;
            this.DateSent = ticket.DateSent.ToLocalDateTime();
            this.SenderProfileName = ticket.SenderProfileName;
            this.Subject = ticket.Subject;
            this.Type = ticket.Type;
            this.ViolationDate = ticket.ViolationDate.ToLocalDateTime();
            this.Status = ticket.Status;
            this.StatusSeenDate = ticket.SeenDate?.ToLocalDateTime();
        }

        public int MessageId { get; set; }

        public DateTime DateSent { get; set; }

        public string SenderProfileName { get; set; }

        public string Subject {  get; set; }

        public string Type { get; set; }

        public DateTime ViolationDate { get; set; }

        public TicketStatusStatus Status { get; set; }

        public DateTime? StatusSeenDate { get; set; }

        public string GetStatus
        {
            get
            {
                switch (this.Status)
                {
                    case TicketStatusStatus.NonServed:
                        return "Невръчен";
                    case TicketStatusStatus.InternallyServed:
                        return $"Връчен през ССЕВ";
                    case TicketStatusStatus.ExternallyServed:
                        return $"Връчен извън ССЕВ";
                    case TicketStatusStatus.Annulled:
                        return $"Анулиран";

                    default:
                        return string.Empty;
                }
            }
        }
    }
}
