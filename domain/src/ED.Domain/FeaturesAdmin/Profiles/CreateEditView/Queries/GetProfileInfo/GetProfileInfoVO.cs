using System;

namespace ED.Domain
{
    public partial interface IAdminProfilesCreateEditViewQueryRepository
    {
        public record GetProfileInfoVO(
            bool CanBeActivated,
            bool IsActive,
            GetProfileInfoVOIndividualInfo? IndividualInfo,
            GetProfileInfoVOLegalEntityInfo? LegalEntityInfo,
            string Identifier,
            string CreatedBy,
            DateTime DateCreated,
            string Phone,
            string EmailAddress,
            string? AddressCountry,
            string? AddressState,
            string? AddressCity,
            string? AddressResidence,
            int TargetGroupId,
            string TargetGroupName,
            bool? EnableMessagesWithCode,
            GetProfileInfoVOBlob[] Documents,
            GetProfileInfoVOLogin[] Logins,
            GetProfileInfoVOProfile[] Profiles,
            GetProfileInfoVORegistrationRequest[] RegistrationRequests,
            bool IsReadOnly,
            bool IsPassive,
            GetProfileInfoVODefaultLogin? DefaultLogin,
            GetProfileInfoVOQuota Quota,
            GetProfileInfoVOEsbUser EsbUser);

        public record GetProfileInfoVOIndividualInfo(
            string FirstName,
            string MiddleName,
            string LastName);

        public record GetProfileInfoVOLegalEntityInfo(
            string Name);

        public record GetProfileInfoVOBlob(
            int BlobId,
            string FileName,
            string? Description,
            DateTime CreateDate,
            string CreatedBy);

        public record GetProfileInfoVODefaultLogin(
            int LoginId,
            bool IsActive,
            string? CertificateThumbprint,
            bool? CanSendOnBehalfOf,
            string? PushNotificationsUrl,
            bool SmsNotificationActive,
            bool SmsNotificationOnDeliveryActive,
            bool EmailNotificationActive,
            bool EmailNotificationOnDeliveryActive,
            bool ViberNotificationActive,
            bool ViberNotificationOnDeliveryActive,
            string Email,
            string Phone);

        public record GetProfileInfoVOLogin(
            int LoginId,
            int ProfileId,
            bool IsDefault,
            string ElectronicSubjectName,
            string AccessGrantedByElectronicSubjectName,
            DateTime AccessGrantedOn);

        public record GetProfileInfoVOProfile(
            int ProfileId,
            string ProfileName,
            string AccessGrantedBy,
            string TargetGroupName,
            bool IsActive);

        public record GetProfileInfoVORegistrationRequest(
            int RegistrationRequestId,
            DateTime CreateDate,
            RegistrationRequestStatus Status);

        public record GetProfileInfoVOQuota(
            int StorageQuotaInMb);

        public record GetProfileInfoVOEsbUser(
            string? OId,
            string? ClientId);
    }
}
