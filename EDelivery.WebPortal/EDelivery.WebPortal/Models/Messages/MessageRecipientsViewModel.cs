using System;

namespace EDelivery.WebPortal.Models.Messages
{
    public class MessageRecipientsViewModel
    {
        public MessageRecipientsViewModel(
            ED.DomainServices.Messages.GetMessageRecipientsResponse.Types.Recipient recipient)
        {
            this.ProfileName = recipient.ProfileName;
            this.DateReceived = recipient.DateReceived?.ToLocalDateTime();
        }

        public string ProfileName { get; set; }

        public DateTime? DateReceived { get; set; }
    }
}