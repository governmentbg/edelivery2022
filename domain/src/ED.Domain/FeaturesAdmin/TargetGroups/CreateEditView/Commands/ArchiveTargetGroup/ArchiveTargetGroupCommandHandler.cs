using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record ArchiveTargetGroupCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<TargetGroup> TargetGroupAggregateRepository)
        : IRequestHandler<ArchiveTargetGroupCommand, Unit>
    {
        public async Task<Unit> Handle(
            ArchiveTargetGroupCommand command,
            CancellationToken ct)
        {
            TargetGroup targetGroup =
                await this.TargetGroupAggregateRepository.FindAsync(
                    command.TargetGroupId,
                    ct);

            targetGroup.Archive(command.AdminUserId);

            // TODO: remove from target group matrix?

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
