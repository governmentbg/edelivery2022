using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record UnpublishTemplateCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Template> TemplateAggregateRepository)
        : IRequestHandler<UnpublishTemplateCommand, Unit>
    {
        public async Task<Unit> Handle(
            UnpublishTemplateCommand command,
            CancellationToken ct)
        {
            Template template =
                await this.TemplateAggregateRepository.FindAsync(
                    command.TemplateId,
                    ct);

            template.Unpublish(command.AdminUserId);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
