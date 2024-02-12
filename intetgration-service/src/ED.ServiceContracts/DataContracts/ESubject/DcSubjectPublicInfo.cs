using EDelivery.Common.Enums;
using System;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts.ESubject
{
    /// <summary>
    /// Public information for electronic subjects
    /// </summary>
    [DataContract]
    [KnownType(typeof(DcInstitutionInfo))]
    [KnownType(typeof(DcLegalPersonInfo))]
    [KnownType(typeof(DcPersonInfo))]
    [KnownType(typeof(DcSubjectInfo))]
    public class DcSubjectPublicInfo
    {
        [DataMember]
        public eProfileType ProfileType { get; set; }

        [DataMember]
        public Guid ElectronicSubjectId { get; set; }

        [DataMember]
        public string ElectronicSubjectName { get; set; }

        [DataMember]
        public string PhoneNumber { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public bool IsActivated { get; set; }
    }
}
