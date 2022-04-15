using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record CreateOrUpdateTemplatePermissionsCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Template> TemplateAggregateRepository)
        : IRequestHandler<CreateOrUpdateTemplatePermissionsCommand, Unit>
    {
        public async Task<Unit> Handle(
            CreateOrUpdateTemplatePermissionsCommand command,
            CancellationToken ct)
        {
            Template template =
                await this.TemplateAggregateRepository.FindAsync(
                    command.TemplateId,
                    ct);

            template.UpdateProfilePermissions(
                command.ProfileIds,
                command.CanSend,
                command.CanReceive);

            template.UpdateTargetGroupPermissions(
                command.TargetGroupIds,
                command.CanSend,
                command.CanReceive);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
