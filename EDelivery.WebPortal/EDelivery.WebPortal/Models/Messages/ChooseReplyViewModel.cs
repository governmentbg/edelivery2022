using System.Collections.Generic;

using EDelivery.WebPortal.Models.Templates;

namespace EDelivery.WebPortal.Models.Messages
{
    public class ChooseReplyViewModel
    {
        public int MessageId { get; set; }
        public List<MessageTemplateInfoViewModel> Templates { get; set; }
    }
}