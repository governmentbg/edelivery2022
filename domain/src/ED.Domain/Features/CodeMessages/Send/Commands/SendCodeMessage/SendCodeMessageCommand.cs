using MediatR;

namespace ED.Domain
{
    public record SendCodeMessageCommand(
        string Identifier,
        string FirstName,
        string MiddleName,
        string LastName,
        string Phone,
        string Email,
        int SenderProfileId,
        int SenderLoginId,
        int TemplateId,
        string Subject,
        string Body,
        string MetaFields,
        int CreatedBy,
        int[] BlobIds)
        : IRequest;
}
