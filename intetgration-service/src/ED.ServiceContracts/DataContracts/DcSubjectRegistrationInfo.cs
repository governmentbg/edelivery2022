using EDelivery.Common.DataContracts.ESubject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    [KnownType(typeof(DcSubjectPublicInfo))]
    public class DcSubjectRegistrationInfo
    {
        public DcSubjectRegistrationInfo(string identificator)
        {
            Identificator = identificator;
        }

        [DataMember]
        public string Identificator { get; set; }

        [DataMember]
        public bool HasRegistration { get; set; }

        [DataMember]
        public DcRegisteredSubjectInfo SubjectInfo { get; set; }
    }

    [DataContract]
    public class DcRegisteredSubjectInfo:DcSubjectPublicInfo
    {
        [DataMember]
        public Common.Enums.eInstitutionType? InstitutionType { get; set; }
    }
}
