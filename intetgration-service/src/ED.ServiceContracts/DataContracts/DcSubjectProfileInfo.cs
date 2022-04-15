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
    [KnownType(typeof(DcAdministrativeActInfo))]
    [KnownType(typeof(DcCertificateInfo))]
    [KnownType(typeof(DcInstitutionInfo))]
    [KnownType(typeof(DcSubjectRequestInfo))]
    [KnownType(typeof(DcLegalPersonInfo))]
    [KnownType(typeof(DcPersonInfo))]
    [KnownType(typeof(DcElectronicIdentityInfo))]
    [KnownType(typeof(DcLoginAccess))]
    [KnownType(typeof(DcProfile))]
    [KnownType(typeof(DcLogin))]
    public class DcSubjectProfileInfo
    {
        [DataMember]
        public DcSubjectInfo Subject { get; set; }
        
        [DataMember]
        public DcLogin Login { get; set; }

        [DataMember]
        public DcProfile Profile { get; set; }

        [DataMember]
        public DcSubjectRequestShortInfo RequestInfo { get; set; }

        [DataMember]
        public List<DcLoginAccess> ProfileLogins { get; set; }

        [DataMember]
        public List<DcProfile> Profiles { get; set; }
    }
}
