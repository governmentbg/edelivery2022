using System;

namespace ED.Domain
{
    public partial interface IAdminRegistrationsListQueryRepository
    {
        public record GetRegistrationRequestsVO(
            int RegistrationRequestId,
            RegistrationRequestStatus RegistrationRequestStatus,
            int ProfileId,
            string ProfileName,
            string AuthorLoginName,
            DateTime CreateDate);
    }
}
