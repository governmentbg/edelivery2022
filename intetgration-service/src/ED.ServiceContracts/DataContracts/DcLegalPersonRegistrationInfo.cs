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
    public class DcLegalPersonRegistrationInfo
    {
        public DcLegalPersonRegistrationInfo()
        {

        }
        public DcLegalPersonRegistrationInfo(string eik)
        {
            EIK = eik;
        }

        [DataMember]
        public string EIK { get; set; }

        [DataMember]
        public bool HasRegistration { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Phone { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public List<DcSubjectShortInfo> ProfilesWithAccess { get; set; }
    }
}
