using System;

namespace ED.Domain
{
    public partial interface IMessageOpenQueryRepository
    {
        public record GetPdfAsSenderVO(
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
            GetPdfAsSenderVORecipients[] Recipients,
            GetPdfAsSenderVOBlobs[] Blobs);

        public record GetPdfAsSenderVORecipients(
            string ProfileName,
            DateTime? DateReceived,
            byte[]? MessageSummary);

        public record GetPdfAsSenderVOBlobs(
            string FileName,
            string Hash,
            string HashAlgorithm,
            long? Size);
    }
}
