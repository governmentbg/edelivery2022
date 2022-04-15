using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.ICodeMessageOpenQueryRepository;

namespace ED.Domain
{
    partial class CodeMessageOpenQueryRepository : ICodeMessageOpenQueryRepository
    {
        public async Task<GetMessageAccessKeyVO> GetMessageAccessKeyAsync(
            int messageId,
            int profileId,
            CancellationToken ct)
        {
            GetMessageAccessKeyVO vos = await (
                from mak in this.DbContext.Set<MessageAccessKey>()

                where mak.MessageId == messageId
                    && mak.ProfileId == profileId

                select new GetMessageAccessKeyVO(
                    mak.ProfileKeyId,
                    mak.EncryptedKey))
                .FirstAsync(ct);

            return vos;
        }
    }
}
