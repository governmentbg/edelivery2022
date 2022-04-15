namespace EDelivery.WebPortal.Models
{
    public class AddRecipientGroupMemberPostViewModel
    {
        public AddRecipientGroupMemberPostViewModel()
        {
        }

        public int RecipientGroupId { get; set; }

        public string ProfileIds { get; set; }
    }
}