using System.ComponentModel.DataAnnotations;

using EDelivery.WebPortal.Utils.Attributes;

namespace EDelivery.WebPortal.Models
{
    public class CreateRecipientGroupViewModel
    {
        public CreateRecipientGroupViewModel()
        {
        }

        public CreateRecipientGroupViewModel(int profileId)
        {
            this.ProfileId = profileId;
        }

        [RequiredRes]
        [StringLength(100, ErrorMessageResourceType = typeof(EDeliveryResources.ErrorMessages), ErrorMessageResourceName = "ErrorMessageFieldLength")]
        public string Name { get; set; }

        [Required]
        public int ProfileId { get; set; }
    }
}