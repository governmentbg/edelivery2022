using System;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts.ESubject
{
    [DataContract]
    public class DcLegalPersonInfo : DcSubjectInfo
    {
        public DcLegalPersonInfo()
        {
            this.ProfileType = Enums.eProfileType.LegalPerson;
        }

        [DataMember]
        public string CompanyName { get; set; }

        [DataMember]
        public DcPersonInfo RegisteredBy { get; set; }

        [DataMember]
        public DateTime InForceDate { get; set; }

        [DataMember]
        public DateTime? DateOutOfForce { get; set; }
    }
}
