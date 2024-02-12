using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IEsbTicketsSendQueryRepository;

namespace ED.Domain
{
    partial class EsbTicketsSendQueryRepository : IEsbTicketsSendQueryRepository
    {
        public async Task<GetProfileContactsVO> GetProfileContactsAsync(
            int profileId,
            CancellationToken ct)
        {
            GetProfileContactsVO vo = await (
                from p in this.DbContext.Set<Profile>()

                where p.Id == profileId

                select new GetProfileContactsVO(
                    p.EmailAddress,
                    p.Phone))
                .SingleAsync(ct);

            return vo;
        }
    }
}
