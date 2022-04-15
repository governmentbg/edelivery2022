using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EDelivery.WebPortal.Models
{
    public class SEOSDocumentDetailsModel
    {
        public Guid ElectronicSubjectId { get; set; }

        public Guid ReceiverGuid { get; set; }

        [Required(ErrorMessageResourceType = typeof(EDeliveryResources.SEOS), ErrorMessageResourceName = "ErrorRequired")]
        public string Receiver { get; set; }

        [Required(ErrorMessageResourceType = typeof(EDeliveryResources.SEOS), ErrorMessageResourceName = "ErrorRequired")]
        public string Subject { get; set; }

        [Required(ErrorMessageResourceType = typeof(EDeliveryResources.SEOS), ErrorMessageResourceName = "ErrorRequired")]
        public string DocumentKind { get; set; }

        public string ReferenceNumber { get; set; }

        public string DocumentAttentionTo { get; set; }

        public string DocumentComment { get; set; }

        [DisplayFormat(DataFormatString = SystemConstants.DatePickerDateFormat)]
        public DateTime? DocumentRequestCloseDate { get; set; }

        [Required(ErrorMessageResourceType = typeof(EDeliveryResources.SEOS), ErrorMessageResourceName = "ErrorDocumenntRequired")]
        //[FileSize(100, ErrorMessageResourceName = "ErrorMaxFileLength", ErrorMessageResourceType = typeof(EDeliveryResources.SEOS))]
        public int DocumenAttachmentFirstContent { get; set; }

        public string DocumenAttachmentFirstFileName { get; set; }

        public string DocumenAttachmentFirstComment { get; set; }

        public List<SEOSDocumentAttachmentModel> DocumenAttachments { get; set; }
    }

}