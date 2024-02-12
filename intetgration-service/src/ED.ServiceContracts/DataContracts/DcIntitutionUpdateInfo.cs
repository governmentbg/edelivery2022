using EDelivery.Common.DataContracts.ESubject;
using System;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcInstitutionUpdateInfo
    {
        [DataMember]
        public bool IsActive { get; set; }
        [DataMember]
        public Guid ElectronicSubjectId { get; set; }
        [DataMember]
        public int ProfileId { get; set; }
        [DataMember]
        public DcAddress Address { get; set; }
        [DataMember]
        public string EmailAddress { get; set; }
        [DataMember]
        public string PhoneNumber { get; set; }
        [DataMember]
        public string InstitutionName { get; set; }
        [DataMember]
        public string EIK { get; set; }
        [DataMember]
        public int TypeId { get; set; }
        [DataMember]
        public Guid? HeadInstitutionId { get; set; }
        [DataMember]
        public string HeadInstitutionName { get; set; }
        [DataMember]
        public DateTime CreatedOn { get; set; }
        [DataMember]
        public string CreatedBy { get; set; }
        [DataMember]
        public bool IsReadOnly { get; set; }
        [DataMember]
        public bool IsSendMessageWithCodeEnabled { get; set; }
        [DataMember]
        public DcSubjectRegistrationDocument RegistrationDocument { get; set; }
    }
}
