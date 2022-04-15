using System.ComponentModel.DataAnnotations;

using EDelivery.WebPortal.Utils.Attributes;

namespace EDelivery.WebPortal.Models
{
    public class EditRecipientGroupViewModel
    {
        public EditRecipientGroupViewModel()
        {
        }

        public EditRecipientGroupViewModel(
            ED.DomainServices.Profiles.GetRecipientGroupResponse recipientGroup)
        {
            this.RecipientGroupId = recipientGroup.RecipientGroupId;
            this.Name = recipientGroup.Name;
        }

        public int RecipientGroupId { get; set; }

        [RequiredRes]
        [StringLength(100, ErrorMessageResourceType = typeof(EDeliveryResources.ErrorMessages), ErrorMessageResourceName = "ErrorMessageFieldLength")]
        public string Name { get; set; }
    }
}