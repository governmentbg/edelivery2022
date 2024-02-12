using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public interface IProfileBlobAccessKeyAggregateRepository : IAggregateRepository<ProfileBlobAccessKey>
    {
        Task<ProfileBlobAccessKey?> FindAsync(int profileId, int blobId, ProfileBlobAccessKeyType type, CancellationToken ct);
    }
}
