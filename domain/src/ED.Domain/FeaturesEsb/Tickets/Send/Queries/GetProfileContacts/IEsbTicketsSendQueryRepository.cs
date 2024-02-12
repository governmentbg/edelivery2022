using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbTicketsSendQueryRepository
    {
        Task<GetProfileContactsVO> GetProfileContactsAsync(
            int profileId,
            CancellationToken ct);
    }
}
