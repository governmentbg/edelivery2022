namespace EDelivery.WebPortal.Models
{
    public class InboxViewModel
    {
        public InboxViewModel(int targetGroupId)
        {
            this.TargetGroupId = targetGroupId;
        }

        public int TargetGroupId { get; set; }

        public SearchMessagesViewModel SearchFilter { get; set; }

        public PagedList.PagedListLight<ED.DomainServices.Messages.InboxResponse.Types.Message> Messages { get; set; }
    }
}