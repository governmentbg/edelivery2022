using System.ComponentModel.DataAnnotations;

using EDelivery.WebPortal.Utils.Attributes;

namespace EDelivery.WebPortal.Models
{
    public class SearchPersonModel
    {
        [RequiredRes]
        [RegularExpression(SystemConstants.CyrilicPattern, ErrorMessageResourceName = "InfoPersonalData", ErrorMessageResourceType = typeof(EDeliveryResources.Common))]
        public string FirstName { get; set; }

        [RequiredRes]
        [RegularExpression(SystemConstants.CyrilicPattern, ErrorMessageResourceName = "InfoPersonalData", ErrorMessageResourceType = typeof(EDeliveryResources.Common))]
        public string LastName { get; set; }

        [RequiredRes]
        public string Identifier { get; set; }

        public int SelectedIndividualProfileId { get; set; }

        public string SelectedIndividualProfileName { get; set; }
    }
}