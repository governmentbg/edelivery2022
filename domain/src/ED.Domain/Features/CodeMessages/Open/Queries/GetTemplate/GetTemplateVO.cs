namespace ED.Domain
{
    public partial interface ICodeMessageOpenQueryRepository
    {
        public record GetTemplateVO(
            int BlobId,
            string Content,
            string? SenderDocumentField,
            string? RecipientDocumentField,
            string? SubjectDocumentField,
            string? DateSentDocumentField,
            string? DateReceivedDocumentField,
            string? SenderSignatureDocumentField,
            string? RecipientSignatureDocumentField);
    }
}
