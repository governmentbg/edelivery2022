using System;
using System.Runtime.Serialization;

namespace EDelivery.SEOS.DataContracts
{
    [DataContract]
    public class CorespondentRequest
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string City { get; set; }
        [DataMember]
        public string Address { get; set; }
        [DataMember]
        public string Bulstat { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string Phone { get; set; }
        [DataMember]
        public string MobilePhone { get; set; }
    }
}
