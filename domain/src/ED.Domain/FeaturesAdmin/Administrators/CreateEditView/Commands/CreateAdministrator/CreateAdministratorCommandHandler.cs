using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record CreateAdministratorCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<AdminUser> AdminUserAggregateRepository,
        IAdminAdministratorsCreateEditViewQueryRepository AdminAdministratorsRegisterQueryRepository)
        : IRequestHandler<CreateAdministratorCommand, CreateAdministratorCommandResult>
    {
        public async Task<CreateAdministratorCommandResult> Handle(
            CreateAdministratorCommand command,
            CancellationToken ct)
        {
            bool hasExistingAdministrator =
                await this.AdminAdministratorsRegisterQueryRepository
                    .HasExistingAdministratorAsync(
                        command.UserName,
                        ct);

            if (hasExistingAdministrator)
            {
                return new CreateAdministratorCommandResult(
                    false,
                    null,
                    "Duplicate username.");
            }

            AdminUser adminUser = new(
                command.FirstName,
                command.MiddleName,
                command.LastName,
                command.Identifier,
                command.Phone,
                command.Email,
                command.UserName,
                command.PasswordHash,
                command.AdminUserId);

            await this.AdminUserAggregateRepository.AddAsync(adminUser, ct);

            await this.UnitOfWork.SaveAsync(ct);

            return new CreateAdministratorCommandResult(
                true,
                adminUser.Id,
                string.Empty);
        }
    }
}
