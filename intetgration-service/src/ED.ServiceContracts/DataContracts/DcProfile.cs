using EDelivery.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcProfile
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public bool IsDefault { get; set; }

        [DataMember]
        public eProfileType ProfileType { get; set; }

        [DataMember]
        public Guid ElectronicSubjectId { get; set; }

        [DataMember]
        public string ElectronicSubjectName { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string Phone { get; set; }
        
        [DataMember]
        public DateTime DateCreated { get; set; }
        public string CertificateThumbprint { get; set; }
    }
}
