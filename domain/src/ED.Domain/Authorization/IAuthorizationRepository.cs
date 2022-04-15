using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAuthorizationRepository
    {
        Task<bool> ExistsLoginProfileAsync(
            int loginId,
            int profileId,
            CancellationToken ct);

        Task<bool> IsProfileReadOnlyAsync(
            int profileId,
            CancellationToken ct);

        Task<bool> IsMessageSenderAsync(
            int profileId,
            int messageId,
            CancellationToken ct);

        Task<bool> IsMessageRecipientAsync(
            int profileId,
            int messageId,
            CancellationToken ct);

        Task<int?> GetTemplateIdAsync(int messageId, CancellationToken ct);

        Task<int?> GetForwardedMessageTemplateId(int messageId, CancellationToken ct);

        Task<int> GetResponseTemplateIdAsync(int messageId, CancellationToken ct);

        Task<bool> HasMessageAccessKeyAsync(
            int profileId,
            int messageId,
            CancellationToken ct);

        Task<bool> HasReadProfileMessageAccessAsync(
            int profileId,
            int loginId,
            int templateId,
            CancellationToken ct);

        Task<bool> HasWriteProfileMessageAccessAsync(
            int profileId,
            int loginId,
            int templateId,
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

        Task<bool> HasWriteProfileCodeMessageAccessAsync(
            int profileId,
            int loginId,
            CancellationToken ct);
    }
}
