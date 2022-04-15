using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record UpdateAdminProfileCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<AdminUser> AdminUserAggregateRepository)
        : IRequestHandler<UpdateAdminProfileCommand, Unit>
    {
        public async Task<Unit> Handle(
            UpdateAdminProfileCommand command,
            CancellationToken ct)
        {
            AdminUser adminUser =
                await this.AdminUserAggregateRepository
                    .FindAsync(command.Id, ct);

            adminUser.Update(
                command.FirstName,
                command.MiddleName,
                command.LastName,
                command.Identifier,
                command.Phone,
                command.Email);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
