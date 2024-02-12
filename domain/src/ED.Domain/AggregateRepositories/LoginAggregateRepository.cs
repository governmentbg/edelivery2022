using System;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    // todo: use interface for DI
    internal class LoginAggregateRepository : AggregateRepository<Login>
    {
        public LoginAggregateRepository(UnitOfWork unitOfWork)
            : base(unitOfWork)
        { }

        public async Task<Login?> FindLoginByElectronicSubjectId(
            Guid electronicSubjectId,
            CancellationToken ct)
        {
            return await this.FindEntityOrDefaultAsync(
                e => e.ElectronicSubjectId == electronicSubjectId,
                ct);
        }
    }
}
