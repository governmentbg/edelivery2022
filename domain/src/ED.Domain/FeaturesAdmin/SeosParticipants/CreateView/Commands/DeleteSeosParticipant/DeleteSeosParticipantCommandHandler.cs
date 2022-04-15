using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    internal record DeleteSeosParticipantCommandHandler(
            UnitOfWork UnitOfWork)
        : IRequestHandler<DeleteSeosParticipantCommand, Unit>
    {
        public async Task<Unit> Handle(
            DeleteSeosParticipantCommand command,
            CancellationToken ct)
        {
            var query =
$@"
DELETE FROM 
    ElectronicDeliverySEOS.dbo.AS4RegisteredEntity
WHERE 
    Id = @id
";
            _ = await this.UnitOfWork.DbContext.Database.ExecuteSqlRawAsync(
                query,
                new[]
                {
                    new SqlParameter("id", command.ParticipantId),
                },
                cancellationToken: ct);

            return default;
        }
    }
}
