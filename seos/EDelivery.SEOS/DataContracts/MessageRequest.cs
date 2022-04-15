using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDelivery.SEOS.DataContracts
{
    [DataContract]
    public class MessageRequest
    {
        [DataMember]
        public Guid ElectronicSubjectId { get; set; }
        [DataMember]
        public Guid ReceiverGuid { get; set; }
        [DataMember]
        public string Receiver { get; set; }
        [DataMember]
        public string Subject { get; set; }
        [DataMember]
        public string DocumentKind { get; set; }
        [DataMember]
        public string ReferenceNumber { get; set; }
        [DataMember]
        public string DocumentAttentionTo { get; set; }
        [DataMember]
        public string DocumentComment { get; set; }
        [DataMember]
        public DateTime? DocumentRequestCloseDate { get; set; }
        [DataMember]
        public int DocumenAttachmentFirstContent { get; set; }
        [DataMember]
        public string DocumenAttachmentFirstFileName { get; set; }
        [DataMember]
        public string DocumenAttachmentFirstComment { get; set; }
        [DataMember]
        public List<AttachmentRequest> DocumenAttachments { get; set; }
        [DataMember]
        public string ProfileIdentifier { get; set; }
        [DataMember]
        public Guid ProfileGuid { get; set; }
        [DataMember]
        public string LoginProfileName { get; set; }
        [DataMember]
        public Guid LoginProfileGuid { get; set; }
    }
}
