namespace EDelivery.WebPortal.Models
{
    public class OutboxViewModel
    {
        public OutboxViewModel(int targetGroupId)
        {
            this.TargetGroupId = targetGroupId;
        }

        public int TargetGroupId { get; set; }

        public SearchMessagesViewModel SearchFilter { get; set; }

        public PagedList.PagedListLight<ED.DomainServices.Messages.OutboxResponse.Types.Message> Messages { get; set; }
    }
}