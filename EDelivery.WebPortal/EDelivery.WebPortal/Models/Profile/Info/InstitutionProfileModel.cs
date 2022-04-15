using System;

namespace EDelivery.WebPortal.Models
{
    public class InstitutionProfileModel : BaseProfileViewModel
    {
        public InstitutionSpecificModel Request { get; set; }
    }

    public class InstitutionSpecificModel
    {
        public InstitutionSpecificModel(
            string name,
            string eik,
            Guid? parentGuid,
            string parentName)
        {
            this.InstitutionName = name;
            this.EIK = eik;
            this.HeadInstitutionName = parentName;
            this.HeadInstitutionId = parentGuid;
        }

        public string InstitutionName { get; set; }

        public string EIK { get; set; }

        public Guid? HeadInstitutionId { get; set; }

        public string HeadInstitutionName { get; set; }
    }
}