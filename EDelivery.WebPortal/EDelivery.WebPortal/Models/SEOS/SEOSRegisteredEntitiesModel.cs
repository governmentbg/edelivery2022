using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EDelivery.WebPortal.Models
{
    public class SEOSRegisteredEntitiesModel
    {
        public string SearchPhase { get; set; }
        public List<SEOSEntityModel> Entities { get; set; }
        public bool ShowOnlyActive { get; set; }
    }
}