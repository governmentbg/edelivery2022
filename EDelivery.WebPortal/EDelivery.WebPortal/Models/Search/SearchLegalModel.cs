using EDelivery.WebPortal.Utils.Attributes;

namespace EDelivery.WebPortal.Models
{
    public class SearchLegalModel
    {
        [RequiredRes]
        public string CompanyRegistrationNumber { get; set; }

        public int SelectedLegalEntityProfileId { get; set; }

        public string SelectedLegalEntityProfileName { get; set; }
    }
}