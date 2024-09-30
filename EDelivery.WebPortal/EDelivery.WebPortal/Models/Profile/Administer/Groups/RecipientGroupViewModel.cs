using System;

namespace EDelivery.WebPortal.Models
{
    public class RecipientGroupViewModel
    {
        public RecipientGroupViewModel(
            ED.DomainServices.Profiles.GetRecipientGroupResponse recipientGroup)
        {
            this.RecipientGroupId = recipientGroup.RecipientGroupId;
            this.Name = recipientGroup.Name;
            this.CreateDate = recipientGroup.CreateDate.ToLocalDateTime();
            this.ModifyDate = recipientGroup.ModifyDate.ToLocalDateTime();
        }

        public int RecipientGroupId { get; set; }

        public string Name { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime ModifyDate { get; set; }
    }
}
