using System;

namespace ED.Domain
{
    public partial interface ITranslationsListQueryRepository
    {
        public record GetMessageTranslationVO(
            int MessageTranslationId,
            int MessageId,
            string Subject,
            string SourceLanguage,
            string TargetLanguage,
            DateTime CreateDate,
            DateTime ModifyDate,
            GetMessageTranslationVORequest[] Requests);

        public record GetMessageTranslationVORequest(
            long? RequestId,
            int? SourceBlobId,
            string? SourceBlobFileName,
            int? TargetBlobId,
            string? TargetBlobFileName,
            MessageTranslationRequestStatus Status,
            string ErrorMessage);
    }
}
