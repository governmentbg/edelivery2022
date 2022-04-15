using System.Runtime.Serialization;

namespace EDelivery.SEOS.DataContracts
{
    [DataContract]
    public class SendMessageResult
    {
        [DataMember]
        public string ErrorMessage { get; set; }

        [DataMember]
        public DocumentStatusType Status { get; set; }

        [DataMember]
        public string Request { get; set; }
    }
}
