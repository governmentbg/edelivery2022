using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Common.DataContracts.ESubject
{
    [DataContract]
    [KnownType(typeof(DcAdministrativeActInfo))]
    [KnownType(typeof(DcCertificateInfo))]
    [KnownType(typeof(WebInstitutionInfo))]
    [KnownType(typeof(WebLegalPersonInfo))]
    [KnownType(typeof(DcPersonInfo))]
    [KnownType(typeof(DcDocument))]
    [KnownType(typeof(DcElectronicIdentityInfo))]
    public class DcSubjectRequestInfo:DcSubjectRequestShortInfo
    {
        [DataMember]
        public DcPersonInfo RequestedBySubject { get; set; }

        [DataMember]
        public DcSubjectInfo RequestedForSubject { get; set; }
        
        [DataMember]
        public DcProcessRequestInfo ProcessedInfo { get; set; }
        
        [DataMember]
        public List<IVerificationInfo> VerificationDetailsInfo { get; set; }
        
    }

}
