using System;

namespace ED.Domain
{
    public partial interface ICodeMessageOpenQueryRepository
    {
        public record GetPdfAsRecipientVO(
            int MessageId,
            int? MessagePdfBlobId,
            int SenderProfileId,
            string SenderProfileName,
            DateTime DateSent,
            MessageSummaryVersion? MessageSummaryVersion,
            byte[]? MessageSummary,
            string Subject,
            string? Rnu,
            byte[] Body,
            int? TemplateId,
            byte[] IV,
            GetPdfAsRecipientVORecipient Recipient,
            GetPdfAsRecipientVOBlobs[] Blobs);

        public record GetPdfAsRecipientVORecipient(
            int ProfileId,
            string ProfileName,
            DateTime DateReceived,
            byte[] MessageSummary);

        public record GetPdfAsRecipientVOBlobs(
            string FileName,
            string Hash,
            string HashAlgorithm,
            long? Size);
    }
}
