using System;

namespace ED.Domain
{
    public partial interface IMessageOpenQueryRepository
    {
        public record GetPdfAsRecipientVO(
            int? MessagePdfBlobId,
            int SenderProfileId,
            string SenderProfileName,
            DateTime DateSent,
            MessageSummaryVersion? MessageSummaryVersion,
            byte[]? MessageSummary,
            string Subject,
            byte[] Body,
            int? TemplateId,
            byte[] IV,
            GetPdfAsRecipientVORecipient Recipient,
            GetPdfAsRecipientVOBlobs[] Blobs);

        public record GetPdfAsRecipientVORecipient(
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
