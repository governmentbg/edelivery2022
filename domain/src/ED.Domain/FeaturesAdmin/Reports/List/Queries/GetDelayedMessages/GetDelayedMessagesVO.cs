using System;

namespace ED.Domain
{
    public partial interface IAdminReportsListQueryRepository
    {
        public record GetDelayedMessagesVO(
            int RecipientProfileId,
            string RecipientProfileName,
            bool IsRecipientProfileActivated,
            string RecipientProfileTargetGroupName,
            string RecipientEmail,
            int SenderProfileId,
            string SenderProfileName,
            string SenderEmail,
            string? MessageSubject,
            DateTime? DateSent,
            int Delay);
    }
}
