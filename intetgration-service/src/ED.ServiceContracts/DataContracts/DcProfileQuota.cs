using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcProfileQuota
    {
        [DataMember]
        public int ProfileId { get; set; }

        [DataMember]
        public int MaxSendDocSizeInMb { get; set; }

        [DataMember]
        public int MaxReceiveDocSizeInMb { get; set; }
    }
}