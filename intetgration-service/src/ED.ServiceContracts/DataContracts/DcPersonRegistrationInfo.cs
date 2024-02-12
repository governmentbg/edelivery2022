using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    [KnownType(typeof(DcSubjectShortInfo))]
    public class DcPersonRegistrationInfo
    {
        public DcPersonRegistrationInfo()
        {

        }
        public DcPersonRegistrationInfo(string personIdentificator)
        {
            this.PersonIdentificator = personIdentificator;
        }

        [DataMember]
        public string PersonIdentificator { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool HasRegistration { get; set; }

        [DataMember]
        public List<DcSubjectShortInfo> AccessibleProfiles { get; set; }
    }
}
