using System;

namespace ED.Domain
{
    public partial interface IAdminReportsListQueryRepository
    {
        public record GetSentMessagesVO(
            int SenderProfileId,
            string SenderProfileName,
            bool IsSenderProfileActivated,
            string SenderProfileTargetGroupName,
            int RecipientProfileId,
            string RecipientProfileName,
            bool IsRecipientProfileActivated,
            string RecipientProfileTargetGroupName,
            string MessageSubject,
            DateTime DateSent,
            DateTime? DateReceived);
    }
}
