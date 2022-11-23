using EDelivery.WebPortal.Utils.Attributes;

namespace EDelivery.WebPortal.Models
{
    public class SearchLegalEntityModel
    {
        public SearchLegalEntityModel()
        {
        }

        public SearchLegalEntityModel(int templateId)
        {
            this.TemplateId = templateId;
        }

        [RequiredRes]
        public string CompanyRegistrationNumber { get; set; }

        public int SelectedLegalEntityProfileId { get; set; }

        public string SelectedLegalEntityProfileName { get; set; }

        public int TemplateId { get; set; }
    }
}
