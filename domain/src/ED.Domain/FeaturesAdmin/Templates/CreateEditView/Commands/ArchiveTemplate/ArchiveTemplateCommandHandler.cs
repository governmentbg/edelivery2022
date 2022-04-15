using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record ArchiveTemplateCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Template> TemplateAggregateRepository)
        : IRequestHandler<ArchiveTemplateCommand, int>
    {
        public async Task<int> Handle(
            ArchiveTemplateCommand command,
            CancellationToken ct)
        {
            Template template =
                await this.TemplateAggregateRepository.FindAsync(
                    command.TemplateId,
                    ct);

            template.Archive(command.AdminUserId);

            await this.UnitOfWork.SaveAsync(ct);

            return template.TemplateId;
        }
    }
}
