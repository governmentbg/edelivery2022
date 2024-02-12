using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts.ESubject
{
    [DataContract]
    [KnownType(typeof(DcAdministrativeActInfo))]
    [KnownType(typeof(DcCertificateInfo))]
    [KnownType(typeof(WebDcInstitutionInfo))]
    [KnownType(typeof(WebDcLegalPersonInfo))]
    [KnownType(typeof(DcPersonInfo))]
    [KnownType(typeof(DcDocument))]
    [KnownType(typeof(DcTokenVerificationInfo))]
    [KnownType(typeof(DcElectronicIdentityInfo))]
    public class WebDcSubjectInfo : WebDcSubjectPublicInfo
    {
        [DataMember]
        public string UniqueSubjectIdentifier { get; set; }

        [DataMember]
        public DcAddress Address { get; set; }

        [DataMember]
        public List<IVerificationInfo> VerificationInfo { get; set; }
    }
}
