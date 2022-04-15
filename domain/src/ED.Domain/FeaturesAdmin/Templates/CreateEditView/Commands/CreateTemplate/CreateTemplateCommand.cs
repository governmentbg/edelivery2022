using MediatR;

namespace ED.Domain
{
    public record CreateTemplateCommand(
        string Name,
        string IdentityNumber,
        string Content,
        int? ResponseTemplateId,
        bool IsSystemTemplate,
        int CreatedBy,
        int ReadLoginSecurityLevelId,
        int WriteLoginSecurityLevelId,
        int BlobId,
        string? SenderDocumentField,
        string? RecipientDocumentField,
        string? SubjectDocumentField,
        string? DateSentDocumentField,
        string? DateReceivedDocumentField,
        string? SenderSignatureDocumentField,
        string? RecipientSignatureDocumentField
    ) : IRequest<int>;
}
