using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record CreateTargetGroupCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<TargetGroup> TargetGroupAggregateRepository)
        : IRequestHandler<CreateTargetGroupCommand, int>
    {
        public async Task<int> Handle(
            CreateTargetGroupCommand command,
            CancellationToken ct)
        {
            TargetGroup targetGroup = new(
                command.Name,
                command.AdminUserId);

            await this.TargetGroupAggregateRepository.AddAsync(targetGroup, ct);

            await this.UnitOfWork.SaveAsync(ct);

            return targetGroup.TargetGroupId;
        }
    }
}
