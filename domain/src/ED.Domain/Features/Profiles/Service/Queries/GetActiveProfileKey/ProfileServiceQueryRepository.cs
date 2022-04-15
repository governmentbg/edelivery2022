using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IProfileServiceQueryRepository;

namespace ED.Domain
{
    partial class ProfileServiceQueryRepository : IProfileServiceQueryRepository
    {
        public async Task<GetActiveProfileKeyVO?> GetActiveProfileKeyAsync(
            int profileId,
            CancellationToken ct)
        {
            return await this.DbContext.Set<ProfileKey>()
                .Where(pk => pk.ProfileId == profileId && pk.IsActive)
                .Select(pk =>
                    new GetActiveProfileKeyVO(
                        pk.ProfileKeyId,
                        pk.ExpiresAt,
                        pk.Provider,
                        pk.KeyName,
                        pk.OaepPadding))
                .SingleOrDefaultAsync(ct);
        }
    }
}
