using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    internal record CreateSeosParticipantCommandHandler(
            UnitOfWork UnitOfWork)
        : IRequestHandler<CreateSeosParticipantCommand, Unit>
    {
        public async Task<Unit> Handle(
            CreateSeosParticipantCommand command,
            CancellationToken ct)
        {
            var query =
$@"
INSERT INTO 
    ElectronicDeliverySEOS.dbo.AS4RegisteredEntity(EIK, AS4Node)
VALUES
    (@identifier, @as4node)";

            _ = await this.UnitOfWork.DbContext.Database.ExecuteSqlRawAsync(
                query,
                new[]
                {
                    new SqlParameter("identifier", command.Identifier),
                    new SqlParameter("as4node", command.AS4Node)
                },
                cancellationToken: ct);

            return default;
        }
    }
}
