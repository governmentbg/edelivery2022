using System;

namespace EDelivery.WebPortal.Models
{
    public class LegalProfileModel : BaseProfileViewModel
    {
        public LegalSpecificModel Request { get; set; }
    }

    public class LegalSpecificModel
    {
        public LegalSpecificModel(string name, string registrationNumber)
        {
            this.CompanyName = name;
            this.RegistrationNumber = registrationNumber;
            this.RegisteredBy = string.Empty;
            this.OutOfForceDate = null;
            this.InforceDate = null;
        }

        public string RegisteredBy { get; set; }

        public string CompanyName { get; set; }

        public string RegistrationNumber { get; set; }

        public DateTime? InforceDate { get; set; }

        public DateTime? OutOfForceDate { get; set; }
    }
}