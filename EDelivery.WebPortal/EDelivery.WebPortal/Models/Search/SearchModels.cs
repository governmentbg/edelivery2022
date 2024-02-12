using System.Collections.Generic;
using System.Web.Mvc;

namespace EDelivery.WebPortal.Models
{
    public class SearchModel
    {
        public SearchModel(int templateId)
        {
            this.TemplateId = templateId;
        }

        public int TemplateId { get; set; }

        public bool CanSendToIndividuals { get; set; }

        public bool CanSendToLegalEntities { get; set; }

        public List<SelectListItem> TargetGroups { get; set; }
    }
}
