using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
            PersonIdentificator = personIdentificator;
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
