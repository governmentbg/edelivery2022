using System;

namespace EDelivery.WebPortal.Models
{
    public class RecipientGroupItemViewModel
    {
        public RecipientGroupItemViewModel(
            ED.DomainServices.Profiles.GetRecipientGroupsResponse.Types.RecipientGroup recipientGroup)
        {
            this.RecipientGroupId = recipientGroup.RecipientGroupId;
            this.Name = recipientGroup.Name;
            this.CreateDate = recipientGroup.CreateDate.ToLocalDateTime();
            this.ModifyDate = recipientGroup.ModifyDate.ToLocalDateTime();
            this.NumberOfMembers = recipientGroup.NumberOfMembers;
        }

        public int RecipientGroupId { get; set; }

        public string Name { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime ModifyDate { get; set; }

        public int NumberOfMembers { get; set; }
    }
}