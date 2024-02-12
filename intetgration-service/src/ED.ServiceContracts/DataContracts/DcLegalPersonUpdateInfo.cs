using EDelivery.Common.DataContracts.ESubject;
using System;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcLegalPersonUpdateInfo
    {
        [DataMember]
        public bool IsActive { get; set; }
        [DataMember]
        public Guid ElectronicSubjectId { get; set; }
        [DataMember]
        public int ProfileId { get; set; }
        [DataMember]
        public string EIK { get; set; }
        [DataMember]
        public DcAddress Address { get; set; }
        [DataMember]
        public string EmailAddress { get; set; }
        [DataMember]
        public string PhoneNumber { get; set; }
        [DataMember]
        public string CompanyName { get; set; }
        [DataMember]
        public DateTime RegisteredOn { get; set; }
        [DataMember]
        public string RegisteredBy { get; set; }
        [DataMember]
        public DateTime? DateInforce { get; set; }
        [DataMember]
        public DateTime? DateOutOfForce { get; set; }
        [DataMember]
        public DcSubjectRegistrationDocument RegistrationDocument { get; set; }
    }
}
