using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Common.DataContracts.ESubject
{
    [DataContract]
    [KnownType(typeof(DcDocument))]
    [KnownType(typeof(DcAddress))]
    [KnownType(typeof(DcSubjectPublicInfo))]
    [KnownType(typeof(DcOperationError))]
    public class DcInstitutionRegistrationInfo
    {
        public DcInstitutionRegistrationInfo()
        {
        }

        [DataMember]
        public DcOperationError Error { get; set; }

        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string RegistrationCode { get; set; }
        [DataMember]
        public string PhoneNumber { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public DcAddress Address { get; set; }
        [DataMember]
        public int Type { get; set; }
        [DataMember]
        public DcSubjectRegistrationDocument RegistrationDocument { get; set; }
        [DataMember]
        public DcSubjectPublicInfo ProfileAdministrator { get; set; }
    }
}
