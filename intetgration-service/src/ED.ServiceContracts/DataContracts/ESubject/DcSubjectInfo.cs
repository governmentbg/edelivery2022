using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts.ESubject
{
    [DataContract]
    [KnownType(typeof(DcAdministrativeActInfo))]
    [KnownType(typeof(DcCertificateInfo))]
    [KnownType(typeof(DcInstitutionInfo))]
    [KnownType(typeof(WebInstitutionInfo))]
    [KnownType(typeof(DcLegalPersonInfo))]
    [KnownType(typeof(WebLegalPersonInfo))]
    [KnownType(typeof(DcTokenVerificationInfo))]
    [KnownType(typeof(DcPersonInfo))]
    [KnownType(typeof(DcElectronicIdentityInfo))]
    [KnownType(typeof(DcDocument))]
    public class DcSubjectInfo : DcSubjectPublicInfo
    {
        [DataMember]
        public string UniqueSubjectIdentifier { get; set; }

        [DataMember]
        public DcAddress Address { get; set; }

        [DataMember]
        public List<IVerificationInfo> VerificationInfo { get; set; }

        [DataMember]
        public DateTime? DateCreated { get; set; }
    }
}
