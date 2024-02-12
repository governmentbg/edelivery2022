using EDelivery.WebPortal.Utils.Attributes;

using System.ComponentModel.DataAnnotations;

namespace EDelivery.WebPortal.Models
{
    public class CreateTranslationViewModel
    {
        public CreateTranslationViewModel() { }

        public CreateTranslationViewModel(int messageId)
        {
            this.MessageId = messageId;
        }

        [Required]
        public int MessageId { get; set; }

        [RequiredRes]
        public string SourceLanguage { get; set; }

        [RequiredRes]
        public string TargetLanguage { get; set; }
    }
}
