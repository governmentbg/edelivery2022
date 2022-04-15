using System;
using System.Runtime.Serialization;

namespace EDelivery.SEOS.DataContracts
{
    [DataContract]
    public class ChangeDocumentStatusRequest
    {
        [DataMember]
        public Guid MessageId { get; set; }
        [DataMember]
        public DocumentStatusType NewStatus { get; set; }
        [DataMember]
        public string RejectReason { get; set; }
        [DataMember]
        public DateTime? ExpectedDateClose { get; set; }
        [DataMember]
        public Guid ProfileGuid { get; set; }
    }
}
