using MediatR;

namespace ED.Domain
{
    public record EsbSendMessageCommand(
        int[] RecipientProfileIds,
        int SenderProfileId,
        int SenderLoginId,
        int? SenderViaLoginId,
        int TemplateId,
        string Subject,
        string? ReferencedOrn,
        string? AdditionalIdentifier,
        string Body,
        string MetaFields,
        int CreatedBy,
        int[] BlobIds,
        int? ForwardedMessageId)
        : IRequest<int>;
}
