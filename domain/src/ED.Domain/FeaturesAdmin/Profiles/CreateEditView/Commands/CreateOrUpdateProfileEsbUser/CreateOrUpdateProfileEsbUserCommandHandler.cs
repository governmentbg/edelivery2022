using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record CreateOrUpdateProfileEsbUserCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<ProfileEsbUser> ProfileEsbUserAggregateRepository)
        : IRequestHandler<CreateOrUpdateProfileEsbUserCommand, Unit>
    {
        public async Task<Unit> Handle(
            CreateOrUpdateProfileEsbUserCommand command,
            CancellationToken ct)
        {
            ProfileEsbUser? profileEsbUser =
                await this.ProfileEsbUserAggregateRepository.TryFindAsync(
                    command.ProfileId,
                    ct);

            if (profileEsbUser == null)
            {
                profileEsbUser = new(
                    command.ProfileId,
                    command.OId,
                    command.ClientId,
                    command.AdminUserId);

                await this.ProfileEsbUserAggregateRepository.AddAsync(profileEsbUser, ct);
            }
            else
            {
                profileEsbUser.Update(command.OId, command.ClientId, command.AdminUserId);
            }

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
