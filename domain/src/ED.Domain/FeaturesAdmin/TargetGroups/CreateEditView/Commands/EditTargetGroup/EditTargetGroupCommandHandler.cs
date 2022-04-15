using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record EditTargetGroupCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<TargetGroup> TargetGroupAggregateRepository)
        : IRequestHandler<EditTargetGroupCommand, Unit>
    {
        public async Task<Unit> Handle(
            EditTargetGroupCommand command,
            CancellationToken ct)
        {
            TargetGroup targetGroup =
                await this.TargetGroupAggregateRepository.FindAsync(
                    command.TargetGroupId,
                    ct);

            targetGroup.Update(command.Name, command.AdminUserId);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
