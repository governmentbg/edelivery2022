using EDelivery.WebPortal.SeosService;
using System;

namespace EDelivery.WebPortal.Models
{
    public class SEOSDocumentModel
    {
        public Guid UniqueIdentifier { get; set; }
        public DocumentStatusType Status { get; set; }
        public string RegIndex { get; set; }
        public string Subject { get; set; }
        public Guid? ReceiverId { get; set; }
        public string Receiver { get; set; }
        public Guid? SenderId { get; set; }
        public string Sender { get; set; }
        public DateTime? DateReceived { get; set; }
        public DateTime? DateSent { get; set; }
        public string DocumentKind { get; set; }
        public string DocumentReferenceNumber { get; set; }
    }
}