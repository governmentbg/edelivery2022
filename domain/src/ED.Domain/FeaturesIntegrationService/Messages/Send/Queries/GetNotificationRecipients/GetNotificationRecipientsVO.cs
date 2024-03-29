﻿namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesSendQueryRepository
    {
        public record GetNotificationRecipientsVO(
            int ProfileId,
            string ProfileName,
            int LoginId,
            string LoginName,
            bool IsEmailNotificationEnabled,
            string Email,
            bool IsSmsNotificationEnabled,
            bool IsViberNotificationEnabled,
            string Phone,
            string? PushNotificationUrl);
    }
}
