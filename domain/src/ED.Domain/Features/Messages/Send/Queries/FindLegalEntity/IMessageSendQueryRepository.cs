using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        Task<FindLegalEntityVO?> FindLegalEntityAsync(
            string identifier,
            CancellationToken ct);
    }
}
