using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcIntegrationDetails
    {
        [DataMember]
        public bool Enabled { get; set; }
        [DataMember]
        public int ProfileId { get; set; }
        [DataMember]
        public string CertificateThumbprint { get; set; }
        [DataMember]
        public string PushNotificaitonsUrl { get; set; }
    }
}
