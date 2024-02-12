using System;

namespace ED.Domain
{
    public partial interface IJobsMessagesOpenQueryRepository
    {
        public record GetAsRecipientVO(
            int MessageId,
            DateTime DateSent,
            DateTime? DateReceived,
            string SenderProfileName,
            int TemplateId,
            string Subject,
            string? Rnu,
            byte[] Body,
            int RecipientProfileKeyId,
            byte[] RecipientEncryptedKey,
            byte[] IV,
            string TemplateName,
            GetAsRecipientVOBlob[] Blobs);

        public record GetAsRecipientVOBlob(
            int BlobId,
            string FileName,
            string Hash,
            string HashAlgorithm,
            long? Size,
            string? DocumentRegistrationNumber);
    }
}
