using MediatR;

namespace ED.Domain
{
    public record ArchiveTemplateCommand(int TemplateId, int AdminUserId)
        : IRequest<int>;
}
