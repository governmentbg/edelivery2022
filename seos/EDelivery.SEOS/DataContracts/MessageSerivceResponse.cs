using System.Runtime.Serialization;

namespace EDelivery.SEOS.DataContracts
{
    [DataContract]
    public class MessageSerivceResponse
    {
        [DataMember]
        public string ServiceCode { get; set; }
        [DataMember]
        public string ServiceType { get; set; }
        [DataMember]
        public string ServiceName { get; set; }
    }
}
