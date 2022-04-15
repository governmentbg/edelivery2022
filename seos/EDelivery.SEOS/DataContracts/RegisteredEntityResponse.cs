using System;
using System.Runtime.Serialization;

namespace EDelivery.SEOS.DataContracts
{
    [DataContract]
    public class RegisteredEntityResponse
    {
        [DataMember]
        public Guid UniqueIdentifier { get; set; }
        [DataMember]
        public string AdministrationBodyName { get; set; }
        [DataMember]
        public string EIK { get; set; }
        [DataMember]
        public string Phone { get; set; }
        [DataMember]
        public string Emailddress { get; set; }
        [DataMember]
        public string ServiceUrl { get; set; }
        [DataMember]
        public EntityServiceStatusEnum Status { get; set; }
    }
}
