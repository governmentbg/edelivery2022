using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record ChangePasswordAdminProfileCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<AdminUser> AdminUserAggregateRepository,
        IAdminProfileCreateEditViewQueryRepository AdminProfileCreateEditViewQueryRepository)
        : IRequestHandler<ChangePasswordAdminProfileCommand, Unit>
    {
        public async Task<Unit> Handle(
            ChangePasswordAdminProfileCommand command,
            CancellationToken ct)
        {
            AdminUser adminUser =
                await this.AdminUserAggregateRepository
                    .FindAsync(command.Id, ct);

            adminUser.ChangePassword(command.PasswordHash);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
