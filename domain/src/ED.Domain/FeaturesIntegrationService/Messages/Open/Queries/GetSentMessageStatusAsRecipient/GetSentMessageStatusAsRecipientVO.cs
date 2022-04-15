using System;

namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesOpenQueryRepository
    {
        public record GetSentMessageStatusAsRecipientVO(
            int MessageId,
            string MessageSubject,
            DateTime DateCreated,
            DateTime DateSent,
            DateTime? DateReceived,
            string MessageBody,
            GetSentMessageStatusAsRecipientVOProfile SenderProfile,
            GetSentMessageStatusAsRecipientVOLogin SenderLogin,
            GetSentMessageStatusAsRecipientVOProfile? RecipientProfile,
            GetSentMessageStatusAsRecipientVOLogin? RecipientLogin,
            GetSentMessageStatusAsRecipientVOBlob[] Blobs,
            GetSentMessageStatusAsRecipientVOForwardedMessage? ForwardedMessage,
            bool FirstTimeOpen,
            GetSentMessageStatusAsRecipientVOTimestampContent TimestampNro,
            GetSentMessageStatusAsRecipientVOTimestampContent? TimestampNrd,
            GetSentMessageStatusAsRecipientVOTimestampContent TimestampMessage);

        public record GetSentMessageStatusAsRecipientVOProfile(
            int ProfileId,
            string ProfileSubjectId,
            string ProfileName,
            string Email,
            string? Phone,
            int TargetGroupId,
            DateTime DateCreated);

        public record GetSentMessageStatusAsRecipientVOLogin(
            int LoginId,
            string LoginSubjectId,
            string LoginName,
            string? Email,
            string? Phone,
            bool IsActive,
            string? CertificateThumbprint,
            string? PushNotificationsUrl);

        public record GetSentMessageStatusAsRecipientVOBlob(
            int BlobId,
            string? DocumentRegistrationNumber,
            string FileName,
            byte[] Timestamp);

        public record GetSentMessageStatusAsRecipientVOForwardedMessage(
            int MessageId,
            string MessageSubject,
            DateTime DateCreated,
            DateTime DateSent,
            DateTime? DateReceived,
            string MessageBody,
            GetSentMessageStatusAsRecipientVOProfile SenderProfile,
            GetSentMessageStatusAsRecipientVOLogin SenderLogin,
            GetSentMessageStatusAsRecipientVOProfile? RecipientProfile,
            GetSentMessageStatusAsRecipientVOLogin? RecipientLogin,
            GetSentMessageStatusAsRecipientVOBlob[] Blobs);

        public record GetSentMessageStatusAsRecipientVOTimestampContent(
            byte[] Content,
            string FileName);
    }
}
