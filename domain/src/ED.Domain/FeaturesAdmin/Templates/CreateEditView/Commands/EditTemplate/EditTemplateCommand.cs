using MediatR;

namespace ED.Domain
{
    public record EditTemplateCommand(
        int TemplateId,
        string Name,
        string IdentityNumber,
        string Content,
        int? ResponseTemplateId,
        bool IsSystemTemplate,
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
