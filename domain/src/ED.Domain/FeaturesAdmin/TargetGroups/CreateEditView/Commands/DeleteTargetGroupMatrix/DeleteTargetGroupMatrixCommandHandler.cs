using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record DeleteTargetGroupMatrixCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<TargetGroup> TargetGroupAggregateRepository)
        : IRequestHandler<DeleteTargetGroupMatrixCommand, Unit>
    {
        public async Task<Unit> Handle(
            DeleteTargetGroupMatrixCommand command,
            CancellationToken ct)
        {
            TargetGroup targetGroup =
                await this.TargetGroupAggregateRepository.FindAsync(
                    command.TargetGroupId,
                    ct);

            targetGroup.RemoveRecipientTargetGroup(
                command.RecipientTargetGroupId,
                command.AdminUserId);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
