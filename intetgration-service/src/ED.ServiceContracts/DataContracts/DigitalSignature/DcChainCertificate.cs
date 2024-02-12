using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcChainCertificate
    {
        [DataMember]
        public string Subject { get; set; }

        [DataMember]
        public bool IsRoot { get; set; }
    }
}
