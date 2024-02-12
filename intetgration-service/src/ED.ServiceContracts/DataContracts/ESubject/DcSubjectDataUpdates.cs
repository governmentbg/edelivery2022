using System;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts.ESubject
{
    [DataContract]
    public class DcSubjectDataUpdates
    {
        [DataMember]
        public string PersonFirstName { get; set; }
        [DataMember]
        public string PersonLastName { get; set; }
        [DataMember]
        public string PersonMiddleName { get; set; }
        [DataMember]
        public string PersonIdentifier { get; set; }
        [DataMember]
        public DateTime? PersonBirthDate { get; set; }
        [DataMember]
        public string CompanyName { get; set; }
        [DataMember]
        public string CompanyRegistrationNumber { get; set; }
        [DataMember]
        public string InstitutionName { get; set; }
    }
}
