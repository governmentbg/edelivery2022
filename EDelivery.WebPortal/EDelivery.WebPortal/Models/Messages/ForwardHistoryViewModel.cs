using System;

namespace EDelivery.WebPortal.Models.Messages
{
    public class ForwardHistoryViewModel
    {
        public ForwardHistoryViewModel(
            ED.DomainServices.Messages.GetForwardHistoryResponse.Types.ForwardHistory history)
        {
            this.MessageId = history.MessageId;
            this.SenderName = history.SenderName;
            this.RecipientName = history.RecipientName;
            this.DateSent = history.DateSent.ToLocalDateTime();
            this.DateReceived = history.DateReceived?.ToLocalDateTime();
        }

        public int MessageId { get; set; }
        public string SenderName { get; set; }
        public string RecipientName { get; set; }
        public DateTime DateSent { get; set; }
        public DateTime? DateReceived { get; set; }
    }
}