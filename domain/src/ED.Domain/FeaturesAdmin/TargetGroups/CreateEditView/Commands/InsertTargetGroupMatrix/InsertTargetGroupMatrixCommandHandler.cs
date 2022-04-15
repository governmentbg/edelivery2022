using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record InsertTargetGroupMatrixCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<TargetGroup> TargetGroupAggregateRepository)
        : IRequestHandler<InsertTargetGroupMatrixCommand, Unit>
    {
        public async Task<Unit> Handle(
            InsertTargetGroupMatrixCommand command,
            CancellationToken ct)
        {
            TargetGroup targetGroup =
                await this.TargetGroupAggregateRepository.FindAsync(
                    command.TargetGroupId,
                    ct);

            targetGroup.AddRecipientTargetGroups(
                command.RecipientTargetGroupIds,
                command.AdminUserId);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
