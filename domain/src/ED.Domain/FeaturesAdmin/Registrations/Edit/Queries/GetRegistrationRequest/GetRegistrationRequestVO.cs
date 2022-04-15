using System;

namespace ED.Domain
{
    public partial interface IAdminRegistrationsEditQueryRepository
    {
        public record GetRegistrationRequestVO(
            DateTime RegistrationRequestCreateDate,
            RegistrationRequestStatus RegistrationRequestStatus,
            string RegistrationRequestAuthor,
            string RegistrationRequestEmail,
            string RegistrationRequestPhone,
            DateTime? RegistrationRequestProcessDate,
            string? RegistrationRequestProcessUser,
            string? RegistrationRequestComment,
            int ProfileId,
            string ProfileName,
            string ProfileIdentifier,
            string ProfileEmail,
            string ProfilePhone,
            string ProfileResidence,
            int RegistrationRequestBlobId,
            string RegistrationRequestFileName,
            GetRegistrationRequestVOSignature[] Signatures);

        public record GetRegistrationRequestVOSignature(
            bool IsValid,
            string SignedBy,
            string CertifiedBy,
            DateTime ValidFrom,
            DateTime ValidTo);
    }
}
