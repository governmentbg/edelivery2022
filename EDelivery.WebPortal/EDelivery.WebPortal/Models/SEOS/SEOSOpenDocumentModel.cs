using EDelivery.WebPortal.SeosService;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EDelivery.WebPortal.Models
{
    public class SEOSOpenDocumentModel
    {
        public List<AttachmentResponse> Attachments { get; set; }
        public string AttentionTo { get; set; }
        public string Comment { get; set; }
        public List<CorespondentResponse> Corespondents { get; set; }
        public DateTime? DateReceived { get; set; }
        public DateTime? DateSent { get; set; }
        public string DocAdditionalData { get; set; }
        public Guid DocGuid { get; set; }
        public string DocKind { get; set; }
        public string DocReferenceNumber { get; set; }
        public string ExternalRegIndex { get; set; }
        public string InternalRegIndex { get; set; }
        public bool IsReceived { get; set; }
        public DateTime? LastStatusChangeDate { get; set; }
        public string ReceiverName { get; set; }
        public Guid? ReceiverGuid { get; set; }
        public DateTime? RequestedCloseDate { get; set; }
        public string SenderName { get; set; }
        public Guid? SenderGuid { get; set; }
        public string Subject { get; set; }
        public SEOSSerivceModel Service { get; set; }
        
        public DocumentStatusType Status { get; set; }
        public Guid MessageGuid { get; set; }
        public string RejectedReason { get; set; }
        public string ReceiverLoginName { get; set; }
        public DateTime? DateRegistered { get; set; }
        public string SenderLoginName { get; set; }
    }

    public class SEOSSerivceModel
    {
        public string ServiceCode { get; set; }
        public string ServiceType { get; set; }
        public string ServiceName { get; set; }
    }
}