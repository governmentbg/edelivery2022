using MediatR;

namespace ED.Domain
{
    public record CreateTemplateCommand(
        string Name,
        string IdentityNumber,
        string? Category,
        string Content,
        int? ResponseTemplateId,
        bool IsSystemTemplate,
        int CreatedBy,
        int ReadLoginSecurityLevelId,
        int WriteLoginSecurityLevelId
    ) : IRequest<int>;
}
