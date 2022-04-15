using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts.ESubject
{
    [DataContract]
    public class DcInstitutionInfo : DcSubjectInfo
    {
        public DcInstitutionInfo()
        {
            this.ProfileType = Enums.eProfileType.Institution;
        }
        [DataMember]
        public DcSubjectPublicInfo HeadInstitution { get; set; }

        [DataMember]
        public List<DcSubjectPublicInfo> SubInstitutions { get; set; }

        [DataMember]
        public string Name { get; set; }
    }
}
