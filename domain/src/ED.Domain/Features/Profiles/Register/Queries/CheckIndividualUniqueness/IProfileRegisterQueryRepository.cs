using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IProfileRegisterQueryRepository
    {
        Task<CheckIndividualUniquenessVO> CheckIndividualUniquenessAsync(
            string identifier,
            string email,
            CancellationToken ct);
    }
}
