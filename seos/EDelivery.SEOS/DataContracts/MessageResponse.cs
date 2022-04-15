using System;
using System.Runtime.Serialization;

namespace EDelivery.SEOS.DataContracts
{
    [DataContract]
    public class MessageResponse
    {
        [DataMember]
        public Guid UniqueIdentifier { get; set; }
        [DataMember]
        public DocumentStatusType Status { get; set; }
        [DataMember]
        public string RegIndex { get; set; }
        [DataMember]
        public string Subject { get; set; }
        [DataMember]
        public Guid? ReceiverId { get; set; }
        [DataMember]
        public string Receiver { get; set; }
        [DataMember]
        public Guid? SenderId { get; set; }
        [DataMember]
        public string Sender { get; set; }
        [DataMember]
        public DateTime? DateReceived { get; set; }
        [DataMember]
        public DateTime? DateSent { get; set; }
        [DataMember]
        public string DocumentKind { get; set; }
        [DataMember]
        public string DocumentReferenceNumber { get; set; }
    }
}
