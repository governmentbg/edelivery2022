using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record CreateRegixReportsAuditLogCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<RegixReportsAuditLog> RegixReportsAuditLogAggregateRepository)
        : IRequestHandler<CreateRegixReportsAuditLogCommand, Unit>
    {
        public async Task<Unit> Handle(
            CreateRegixReportsAuditLogCommand command,
            CancellationToken ct)
        {
            RegixReportsAuditLog regixReportsAuditLog = new(
                Guid.Parse(command.Token),
                command.Data,
                command.LoginId,
                command.ProfileId,
                command.DateCreated);

            await this.RegixReportsAuditLogAggregateRepository.AddAsync(
                regixReportsAuditLog,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
