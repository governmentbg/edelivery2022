using EDelivery.WebPortal.SeosService;
using System;

namespace EDelivery.WebPortal.Models
{
    public class SEOSEntityModel
    {
        public Guid UniqueIdentifier { get; set; }
        public string AdministrationBodyName { get; set; }
        public string EIK { get; set; }
        public string Phone { get; set; }
        public string Emailddress { get; set; }
        public string ServiceUrl { get; set; }
        public EntityServiceStatusEnum Status { get; set; }
    }
}