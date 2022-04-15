using System;

namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesOpenQueryRepository
    {
        public record GetReceivedMessageContentVO(
            int MessageId,
            string MessageSubject,
            DateTime DateCreated,
            DateTime DateSent,
            DateTime? DateReceived,
            string MessageBody,
            GetReceivedMessageContentVOProfile SenderProfile,
            GetReceivedMessageContentVOLogin SenderLogin,
            GetReceivedMessageContentVOProfile? RecipientProfile,
            GetReceivedMessageContentVOLogin? RecipientLogin,
            GetReceivedMessageContentVOBlob[] Blobs,
            GetReceivedMessageContentVOForwardedMessage? ForwardedMessage,
            bool FirstTimeOpen,
            GetReceivedMessageContentVOTimestampContent TimestampNro,
            GetReceivedMessageContentVOTimestampContent? TimestampNrd,
            GetReceivedMessageContentVOTimestampContent TimestampMessage);

        public record GetReceivedMessageContentVOProfile(
            int ProfileId,
            string ProfileSubjectId,
            string ProfileName,
            string Email,
            string? Phone,
            int TargetGroupId,
            DateTime DateCreated);

        public record GetReceivedMessageContentVOLogin(
            int LoginId,
            string LoginSubjectId,
            string LoginName,
            string? Email,
            string? Phone,
            bool IsActive,
            string? CertificateThumbprint,
            string? PushNotificationsUrl);

        public record GetReceivedMessageContentVOSignature(
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

        public record GetReceivedMessageContentVOBlob(
            int BlobId,
            string? DocumentRegistrationNumber,
            string FileName,
            byte[] Timestamp,
            GetReceivedMessageContentVOSignature[] Signatures);

        public record GetReceivedMessageContentVOForwardedMessage(
            int MessageId,
            string MessageSubject,
            DateTime DateCreated,
            DateTime DateSent,
            DateTime? DateReceived,
            string MessageBody,
            GetReceivedMessageContentVOProfile SenderProfile,
            GetReceivedMessageContentVOLogin SenderLogin,
            GetReceivedMessageContentVOProfile? RecipientProfile,
            GetReceivedMessageContentVOLogin? RecipientLogin,
            GetReceivedMessageContentVOBlob[] Blobs);

        public record GetReceivedMessageContentVOTimestampContent(
            byte[] Content,
            string FileName);
    }
}
