using System;

namespace ED.Domain
{
    public partial interface IIntegrationServiceProfilesListQueryRepository
    {
        public record GetLoginByIdentifierVO(
            int LoginId,
            string LoginSubjectId,
            string LoginName,
            string? Email,
            string? Phone,
            bool IsActive,
            string CertificateThumbprint,
            string? PushNotificationsUrl,
            GetLoginByIdentifierVOProfiles[] Profiles);

        public record GetLoginByIdentifierVOProfiles(
            int ProfileId,
            bool IsDefault,
            string ProfileSubjectId,
            string ProfileName,
            string Email,
            string? Phone,
            ProfileType ProfileType,
            DateTime DateCreated,
            int TargetGroupId);
    }
}
