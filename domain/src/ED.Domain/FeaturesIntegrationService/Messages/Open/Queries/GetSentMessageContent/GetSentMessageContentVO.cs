using System;

namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesOpenQueryRepository
    {
        public record GetSentMessageContentVO(
            int MessageId,
            string MessageSubject,
            DateTime DateCreated,
            DateTime DateSent,
            DateTime? DateReceived,
            string MessageBody,
            GetSentMessageContentVOProfile SenderProfile,
            GetSentMessageContentVOLogin SenderLogin,
            GetSentMessageContentVOProfile? RecipientProfile,
            GetSentMessageContentVOLogin? RecipientLogin,
            GetSentMessageContentVOBlob[] Blobs,
            GetSentMessageContentVOForwardedMessage? ForwardedMessage,
            bool FirstTimeOpen,
            GetSentMessageContentVOTimestampContent TimestampNro,
            GetSentMessageContentVOTimestampContent? TimestampNrd,
            GetSentMessageContentVOTimestampContent TimestampMessage);

        public record GetSentMessageContentVOProfile(
            int ProfileId,
            string ProfileSubjectId,
            string ProfileName,
            string Email,
            string? Phone,
            int TargetGroupId,
            DateTime DateCreated);

        public record GetSentMessageContentVOLogin(
            int LoginId,
            string LoginSubjectId,
            string LoginName,
            string? Email,
            string? Phone,
            bool IsActive,
            string? CertificateThumbprint,
            string? PushNotificationsUrl);

        public record GetSentMessageContentVOSignature(
            byte[] SigningCertificate,
            bool CoversDocument,
            bool CoversPriorRevision,
            bool IsTimestamp,
            DateTime SignDate,
            bool ValidAtTimeOfSigning,
            string Issuer,
            string Subject,
            string SerialNumber,
            int Version,
            DateTime ValidFrom,
            DateTime ValidTo);

        public record GetSentMessageContentVOBlob(
            int BlobId,
            string? DocumentRegistrationNumber,
            string FileName,
            byte[] Timestamp,
            byte[] Content,
            GetSentMessageContentVOSignature[] Signatures);

        public record GetSentMessageContentVOForwardedMessage(
            int MessageId,
            string MessageSubject,
            DateTime DateCreated,
            DateTime DateSent,
            DateTime? DateReceived,
            string MessageBody,
            GetSentMessageContentVOProfile SenderProfile,
            GetSentMessageContentVOLogin SenderLogin,
            GetSentMessageContentVOProfile? RecipientProfile,
            GetSentMessageContentVOLogin? RecipientLogin,
            GetSentMessageContentVOBlob[] Blobs);

        public record GetSentMessageContentVOTimestampContent(
            byte[] Content,
            string FileName);
    }
}
