using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public interface IAuthorizationService
    {
        Task<bool> HasProfileAccessAsync(
            int profileId,
            int loginId,
            CancellationToken ct);

        Task<bool> IsReadonlyProfileAsync(
            int profileId,
            CancellationToken ct);

        Task<bool> HasReadMessageAsRecipientAccessAsync(
            int profileId,
            int loginId,
            int messageId,
            CancellationToken ct);

        Task<bool> HasReadMessageAsSenderAccessAsync(
            int profileId,
            int loginId,
            int messageId,
            CancellationToken ct);

        Task<bool> HasReadMessageAsSenderOrRecipientAccessAsync(
            int profileId,
            int loginId,
            int messageId,
            CancellationToken ct);

        Task<bool> HasWriteMessageAccessAsync(
            int profileId,
            int loginId,
            int templateId,
            CancellationToken ct);

        Task<bool> HasForwardMessageAccessAsync(
            int profileId,
            int loginId,
            int messageId,
            CancellationToken ct);

        Task<bool> HasAdministerProfileAccessAsync(
            int profileId,
            int loginId,
            CancellationToken ct);

        Task<bool> HasListProfileMessageAccessAsync(
            int profileId,
            int loginId,
            CancellationToken ct);

        Task<bool> HasAdministerProfileRecipientGroupAccessAsync(
            int profileId,
            int recipientGroupId,
            CancellationToken ct);

        Task<bool> HasAccessTargetGroupSearchAsync(
            int profileId,
            int targetGroupId,
            CancellationToken ct);

        Task<bool> HasWriteCodeMessageAccessAsync(
            int profileId,
            int loginId,
            CancellationToken ct);

        Task<bool> HasMessageAccessKeyAsync(
            int profileId,
            int messageId,
            CancellationToken ct);

        Task<bool> HasReadMessageThroughForwardingAsRecipientAccessAsync(
            int profileId,
            int loginId,
            int messageId,
            int? forwardingMessageId,
            CancellationToken ct);
    }
}
