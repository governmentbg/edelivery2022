using System;

namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesListQueryRepository
    {
        public record GetInboxVO(
            int MessageId,
            string Subject,
            DateTime DateCreated,
            DateTime DateSent,
            DateTime? DateReceived,
            GetInboxVOProfile SenderProfile,
            GetInboxVOProfile RecipientProfile,
            GetInboxVOLogin SenderLogin,
            GetInboxVOLogin? RecipientLogin);

        public record GetInboxVOProfile(
            int ProfileId,
            Guid ProfileSubjectId,
            string ProfileName);

        public record GetInboxVOLogin(
            int LoginId,
            Guid LoginSubjectId,
            string LoginName);
    }
}
