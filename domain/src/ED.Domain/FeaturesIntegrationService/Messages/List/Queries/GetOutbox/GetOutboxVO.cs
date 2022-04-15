using System;

namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesListQueryRepository
    {
        public record GetOutboxVO(
            int MessageId,
            string Subject,
            DateTime DateCreated,
            DateTime DateSent,
            DateTime? DateReceived,
            GetOutboxVOProfile SenderProfile,
            GetOutboxVOProfile RecipientProfile,
            GetOutboxVOLogin SenderLogin,
            GetOutboxVOLogin? RecipientLogin);

        public record GetOutboxVOProfile(
            int ProfileId,
            Guid ProfileSubjectId,
            string ProfileName);

        public record GetOutboxVOLogin(
            int LoginId,
            Guid LoginSubjectId,
            string LoginName);
    }
}
