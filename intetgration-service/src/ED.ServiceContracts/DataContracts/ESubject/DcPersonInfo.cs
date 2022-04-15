using System;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts.ESubject
{
    [DataContract]
    public class DcPersonInfo : DcSubjectInfo
    {
        public DcPersonInfo()
        {
            this.ProfileType = Enums.eProfileType.Person;
        }

        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public string MiddleName { get; set; }

        [DataMember]
        public DateTime BirthDate { get; set; }

        [DataMember]
        public DateTime? DateOfDeath { get; set; }
    }
}
