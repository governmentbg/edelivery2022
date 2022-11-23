using MediatR;

namespace ED.Domain
{
    public record UnpublishTemplateCommand(
        int TemplateId,
        int AdminUserId)
        : IRequest;
}
