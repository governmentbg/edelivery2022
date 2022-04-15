using MediatR;

namespace ED.Domain
{
    public record PublishTemplateCommand(int TemplateId, int AdminUserId)
        : IRequest<int>;
}
