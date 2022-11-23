using System.ComponentModel.DataAnnotations;

using EDelivery.WebPortal.Utils.Attributes;

namespace EDelivery.WebPortal.Models.Messages
{
    public class ForwardMessageModel
    {
        public ForwardMessageModel()
        {
        }

        public ForwardMessageModel(
            int messageId,
            ED.DomainServices.Messages.GetForwardMessageInfoResponse info)
        {
            this.ForwardMessageId = messageId;
            this.ForwardSubject = $"FW: {info.Subject}";
            this.Rnu = info.Rnu;
        }

        [Required]
        public int ForwardMessageId { get; set; }

        [RequiredRes]
        public int ForwardRecipientProfileId { get; set; }

        [RequiredRes]
        [StringLength(255, ErrorMessageResourceType = typeof(EDeliveryResources.ErrorMessages), ErrorMessageResourceName = "ErrorMessageFieldLength")]
        public string ForwardSubject { get; set; }

        [StringLength(64, ErrorMessageResourceName = "ErrorMessageFieldLength", ErrorMessageResourceType = typeof(EDeliveryResources.ErrorMessages))]
        public string Rnu { get; set; }

        [Required]
        public int ForwardTemplateId { get; set; }

        public string TemplateValuesAsJson { get; set; }

        public string TemplateErrorsAsJson { get; set; }
    }
}
