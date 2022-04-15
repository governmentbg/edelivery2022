using System;
using System.Runtime.Serialization;

namespace EDelivery.SEOS.DataContracts
{
    [DataContract]
    public class ChangeDocumentStatusResponse
    {
        [DataMember]
        public DocumentStatusType OldStatus { get; set; }
        [DataMember]
        public Guid MessageId { get; set; }
        [DataMember]
        public DocumentStatusType Status { get; set; }
        [DataMember]
        public string RejectReason { get; set; }
        [DataMember]
        public DateTime? ExpectedDateClose { get; set; }
    }
}
