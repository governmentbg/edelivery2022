using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record DeleteTemplateTargetGroupPermissionCommandCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Template> TemplateAggregateRepository)
        : IRequestHandler<DeleteTemplateTargetGroupPermissionCommand, Unit>
    {
        public async Task<Unit> Handle(
            DeleteTemplateTargetGroupPermissionCommand command,
            CancellationToken ct)
        {
            Template template =
                await this.TemplateAggregateRepository.FindAsync(
                    command.TemplateId,
                    ct);

            template.DeleteTargetGroupPermission(command.TargetGroupId);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
