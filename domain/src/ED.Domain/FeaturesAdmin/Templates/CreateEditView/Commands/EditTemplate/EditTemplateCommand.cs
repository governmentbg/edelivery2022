using MediatR;

namespace ED.Domain
{
    public record EditTemplateCommand(
        int TemplateId,
        string Name,
        string IdentityNumber,
        string? Category,
        string Content,
        int? ResponseTemplateId,
        bool IsSystemTemplate,
        int ReadLoginSecurityLevelId,
        int WriteLoginSecurityLevelId
    ) : IRequest<int>;
}
