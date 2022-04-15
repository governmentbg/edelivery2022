using System;

namespace ED.Domain
{
    public partial interface IProfileListQueryRepository
    {
        public record GetLoginProfilesVO(
            int ProfileId,
            int ProfileType,
            string ProfileGuid,
            string ProfileName,
            string Email,
            string Phone,
            string Identifier,
            bool? EnableMessagesWithCode,
            int TargetGroupId,
            bool IsDefault,
            bool IsReadOnly,
            DateTime DateAccessGranted);
    }
}
