namespace ED.Domain
{
    public partial interface IJobsTranslationsListQueryRepository
    {
        public record GetPendingTranslationRequestsVO(
            int MessageId,
            int ProfileId,
            string SourceLanguage,
            string TargetLanguage,
            int? SourceBlobId,
            string FileName,
            long? Size);
    }
}
