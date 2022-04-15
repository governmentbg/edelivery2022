using System.Collections.Generic;
using System.Web.Mvc;

namespace EDelivery.WebPortal.Models
{
    public class SearchModel
    {
        public SearchModel()
        {
        }

        public bool CanSendToIndividuals { get; set; }

        public bool CanSendToLegalEntities { get; set; }

        public List<SelectListItem> RecipientGroups { get; set; }

        public List<SelectListItem> TargetGroups { get; set; }
    }
}