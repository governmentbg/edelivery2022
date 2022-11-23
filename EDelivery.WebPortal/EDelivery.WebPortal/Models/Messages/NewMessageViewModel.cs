using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using EDelivery.WebPortal.Utils.Attributes;

namespace EDelivery.WebPortal.Models.Messages
{
    public class NewMessageViewModel
    {
        [RequiredRes]
        public string RecipientIds { get; set; }

        [RequiredRes]
        public string RecipientNames { get; set; }

        [RequiredRes]
        [StringLength(255, ErrorMessageResourceType = typeof(EDeliveryResources.ErrorMessages), ErrorMessageResourceName = "ErrorMessageFieldLength")]
        public string Subject { get; set; }

        [StringLength(64, ErrorMessageResourceName = "ErrorMessageFieldLength", ErrorMessageResourceType = typeof(EDeliveryResources.ErrorMessages))]
        public string Rnu { get; set; }

        [Required]
        public int TemplateId { get; set; }

        public string TemplateValuesAsJson { get; set; }

        public string TemplateErrorsAsJson { get; set; }

        public int CurrentProfileId { get; set; }

        public IEnumerable<Tuple<string, string>> GetRecipients()
        {
            string[] recipientIds = this.RecipientIds?
                .Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                .ToArray()
                    ?? Array.Empty<string>();

            string[] recipientNames = this.RecipientNames?
                .Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                .ToArray()
                    ?? Array.Empty<string>();

            for (int i = 0; i < recipientIds.Length; i++)
            {
                yield return new Tuple<string, string>(
                    recipientIds[i],
                    recipientNames[i]);
            }
        }
    }
}
