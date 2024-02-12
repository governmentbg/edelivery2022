using System;

namespace ED.Domain
{
    public partial interface ITranslationsListQueryRepository
    {
        public record GetMessageTranslationsVO(
            int MessageTranslationId,
            int MessageId,
            string Subject,
            string SourceLanguage,
            string TargetLanguage,
            DateTime CreateDate,
            DateTime ModifyDate);
    }
}
