using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDelivery.SEOS.DataContracts
{
    [DataContract]
    public class OpenDocumentResponse
    {
        [DataMember]
        public List<AttachmentResponse> Attachments { get; set; }
        [DataMember]
        public string AttentionTo { get; set; }
        [DataMember]
        public string Comment { get; set; }
        [DataMember]
        public List<CorespondentResponse> Corespondents { get; set; }
        [DataMember]
        public DateTime? DateReceived { get; set; }
        [DataMember]
        public DateTime? DateSent { get; set; }
        [DataMember]
        public string DocAdditionalData { get; set; }
        [DataMember]
        public Guid DocGuid { get; set; }
        [DataMember]
        public string DocKind { get; set; }
        [DataMember]
        public string DocReferenceNumber { get; set; }
        [DataMember]
        public string ExternalRegIndex { get; set; }
        [DataMember]
        public string InternalRegIndex { get; set; }
        [DataMember]
        public bool IsReceived { get; set; }
        [DataMember]
        public DateTime? LastStatusChangeDate { get; set; }
        [DataMember]
        public string ReceiverName { get; set; }
        [DataMember]
        public Guid? ReceiverGuid { get; set; }
        [DataMember]
        public DateTime? RequestedCloseDate { get; set; }
        [DataMember]
        public string SenderName { get; set; }
        [DataMember]
        public Guid? SenderGuid { get; set; }
        [DataMember]
        public string Subject { get; set; }
        [DataMember]
        public MessageSerivceResponse Service { get; set; }
        [DataMember]
        public DocumentStatusType Status { get; set; }
        [DataMember]
        public Guid MessageGuid { get; set; }
        [DataMember]
        public string RejectedReason { get; set; }
        [DataMember]
        public string ReceiverLoginName { get; set; }
        [DataMember]
        public DateTime? DateRegistered { get; set; }
        [DataMember]
        public string SenderLoginName { get; set; }
    }
}
