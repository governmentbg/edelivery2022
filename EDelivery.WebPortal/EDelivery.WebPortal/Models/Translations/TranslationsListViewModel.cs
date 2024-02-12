using ED.DomainServices.Translations;

using System;
using System.Collections.Generic;
using System.Linq;

namespace EDelivery.WebPortal.Models
{
    public class TranslationsListViewModel
    {
        public TranslationsListViewModel() { }

        public TranslationsListViewModel(int messageId)
        {
            this.MessageId = messageId;
        }

        public TranslationsListViewModel(
            int messageId,
            GetTranslationsResponse resp)
            : this(messageId)
        {
            this.Items.AddRange(
                resp.Result.Select(e => new TranslationsListItemViewModel(e)));
        }

        public int MessageId { get; set; }

        public List<TranslationsListItemViewModel> Items { get; set; } =
            new List<TranslationsListItemViewModel>();
    }

    public class TranslationsListItemViewModel
    {
        public TranslationsListItemViewModel() { }

        public TranslationsListItemViewModel(
            GetTranslationsResponse.Types.Translation translation)
        {
            this.MessageTranslationId = translation.MessageTranslationId;
            this.MessageId = translation.MessageId;
            this.Subject = translation.Subject;
            this.SourceLanguage = translation.SourceLanguage;
            this.TargetLanguage = translation.TargetLanguage;
            this.CreateDate = translation.CreateDate.ToLocalDateTime();
            this.ModifyDate = translation.ModifyDate.ToLocalDateTime();
        }

        public int MessageTranslationId { get; set; }

        public int MessageId { get; set; }

        public string Subject { get; set; }

        public string SourceLanguage { get; set; }

        public string TargetLanguage { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime ModifyDate { get; set; }
    }
}
