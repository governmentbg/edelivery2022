using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record DeactivateAdministratorCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<AdminUser> AdminUserAggregateRepository)
        : IRequestHandler<DeactivateAdministratorCommand, Unit>
    {
        public async Task<Unit> Handle(
            DeactivateAdministratorCommand command,
            CancellationToken ct)
        {
            AdminUser adminUser =
                await this.AdminUserAggregateRepository
                    .FindAsync(command.Id, ct);

            adminUser.Deactivate(command.AdminUserId);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
