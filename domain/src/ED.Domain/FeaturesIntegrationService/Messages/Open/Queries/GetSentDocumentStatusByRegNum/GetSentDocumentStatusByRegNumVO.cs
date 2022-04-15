using System;

namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesOpenQueryRepository
    {
        public record GetSentDocumentStatusByRegNumVO(
            int MessageId,
            string MessageSubject,
            DateTime DateCreated,
            DateTime DateSent,
            DateTime? DateReceived,
            string MessageBody,
            GetSentDocumentStatusByRegNumVOProfile SenderProfile,
            GetSentDocumentStatusByRegNumVOLogin SenderLogin,
            GetSentDocumentStatusByRegNumVOProfile? RecipientProfile,
            GetSentDocumentStatusByRegNumVOLogin? RecipientLogin,
            GetSentDocumentStatusByRegNumVOBlob[] Blobs,
            GetSentDocumentStatusByRegNumVOForwardedMessage? ForwardedMessage,
            bool FirstTimeOpen,
            GetSentDocumentStatusByRegNumVOTimestampContent TimestampNro,
            GetSentDocumentStatusByRegNumVOTimestampContent? TimestampNrd,
            GetSentDocumentStatusByRegNumVOTimestampContent TimestampMessage);

        public record GetSentDocumentStatusByRegNumVOProfile(
            int ProfileId,
            string ProfileSubjectId,
            string ProfileName,
            string Email,
            string? Phone,
            int TargetGroupId,
            DateTime DateCreated);

        public record GetSentDocumentStatusByRegNumVOLogin(
            int LoginId,
            string LoginSubjectId,
            string LoginName,
            string? Email,
            string? Phone,
            bool IsActive,
            string? CertificateThumbprint,
            string? PushNotificationsUrl);

        public record GetSentDocumentStatusByRegNumVOSignature(
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

        public record GetSentDocumentStatusByRegNumVOBlob(
            int BlobId,
            string? DocumentRegistrationNumber,
            string FileName,
            byte[] Timestamp,
            GetSentDocumentStatusByRegNumVOSignature[] Signatures);

        public record GetSentDocumentStatusByRegNumVOForwardedMessage(
            int MessageId,
            string MessageSubject,
            DateTime DateCreated,
            DateTime DateSent,
            DateTime? DateReceived,
            string MessageBody,
            GetSentDocumentStatusByRegNumVOProfile SenderProfile,
            GetSentDocumentStatusByRegNumVOLogin SenderLogin,
            GetSentDocumentStatusByRegNumVOProfile? RecipientProfile,
            GetSentDocumentStatusByRegNumVOLogin? RecipientLogin,
            GetSentDocumentStatusByRegNumVOBlob[] Blobs);

        public record GetSentDocumentStatusByRegNumVOTimestampContent(
            byte[] Content,
            string FileName);
    }
}
