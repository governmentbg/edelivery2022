using System;

namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesOpenQueryRepository
    {
        public record GetSentMessageStatusAsSenderVO(
            int MessageId,
            string MessageSubject,
            DateTime DateCreated,
            DateTime DateSent,
            DateTime? DateReceived,
            string MessageBody,
            GetSentMessageStatusAsSenderVOProfile SenderProfile,
            GetSentMessageStatusAsSenderVOLogin SenderLogin,
            GetSentMessageStatusAsSenderVOProfile? RecipientProfile,
            GetSentMessageStatusAsSenderVOLogin? RecipientLogin,
            GetSentMessageStatusAsSenderVOBlob[] Blobs,
            GetSentMessageStatusAsSenderVOForwardedMessage? ForwardedMessage,
            bool FirstTimeOpen,
            GetSentMessageStatusAsSenderVOTimestampContent TimestampNro,
            GetSentMessageStatusAsSenderVOTimestampContent? TimestampNrd,
            GetSentMessageStatusAsSenderVOTimestampContent TimestampMessage);

        public record GetSentMessageStatusAsSenderVOProfile(
            int ProfileId,
            string ProfileSubjectId,
            string ProfileName,
            string Email,
            string? Phone,
            int TargetGroupId,
            DateTime DateCreated);

        public record GetSentMessageStatusAsSenderVOLogin(
            int LoginId,
            string LoginSubjectId,
            string LoginName,
            string? Email,
            string? Phone,
            bool IsActive,
            string? CertificateThumbprint,
            string? PushNotificationsUrl);

        public record GetSentMessageStatusAsSenderVOBlob(
            int BlobId,
            string? DocumentRegistrationNumber,
            string FileName,
            byte[] Timestamp);

        public record GetSentMessageStatusAsSenderVOForwardedMessage(
            int MessageId,
            string MessageSubject,
            DateTime DateCreated,
            DateTime DateSent,
            DateTime? DateReceived,
            string MessageBody,
            GetSentMessageStatusAsSenderVOProfile SenderProfile,
            GetSentMessageStatusAsSenderVOLogin SenderLogin,
            GetSentMessageStatusAsSenderVOProfile? RecipientProfile,
            GetSentMessageStatusAsSenderVOLogin? RecipientLogin,
            GetSentMessageStatusAsSenderVOBlob[] Blobs);

        public record GetSentMessageStatusAsSenderVOTimestampContent(
            byte[] Content,
            string FileName);
    }
}
