using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record ActivateAdministratorCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<AdminUser> AdminUserAggregateRepository)
        : IRequestHandler<ActivateAdministratorCommand, Unit>
    {
        public async Task<Unit> Handle(
            ActivateAdministratorCommand command,
            CancellationToken ct)
        {
            AdminUser adminUser =
                await this.AdminUserAggregateRepository
                    .FindAsync(command.Id, ct);

            adminUser.Activate();

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
