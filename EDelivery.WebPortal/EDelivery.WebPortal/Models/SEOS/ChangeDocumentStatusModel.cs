using System;
using System.ComponentModel.DataAnnotations;

using EDelivery.WebPortal.SeosService;
using EDelivery.WebPortal.Utils.Attributes;

namespace EDelivery.WebPortal.Models
{
    public class ChangeDocumentStatusModel
    {
        public DocumentStatusType OldStatus { get; set; }

        public Guid MessageId { get; set; }

        [Required]
        public DocumentStatusType Status { get; set; }

        [RequiredIf("Status", DocumentStatusType.DS_REJECTED, ErrorMessageResourceType = typeof(EDeliveryResources.SEOS), ErrorMessageResourceName = "ErrorRejectReasonMissing")]
        [MaxLength(450, ErrorMessageResourceType = typeof(EDeliveryResources.SEOS), ErrorMessageResourceName = "ErrorExceededMaxLen")]
        public string RejectReason { get; set; }

        [DisplayFormat(DataFormatString = "yyyy-MM-dd")]
        public DateTime? ExpectedDateClose { get; set; }
    }
}