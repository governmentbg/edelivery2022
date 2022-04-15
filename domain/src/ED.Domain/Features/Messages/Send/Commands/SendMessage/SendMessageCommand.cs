using MediatR;

namespace ED.Domain
{
    public record SendMessageCommand(
        int[] RecipientGroupIds,
        int[] RecipientProfileIds,
        int SenderProfileId,
        int SenderLoginId,
        int TemplateId,
        string Subject,
        string? ReferencedOrn,
        string? AdditionalIdentifier,
        string Body,
        string MetaFields,
        int CreatedBy,
        int[] BlobIds,
        int? ForwardedMessageId)
        : IRequest;
}
