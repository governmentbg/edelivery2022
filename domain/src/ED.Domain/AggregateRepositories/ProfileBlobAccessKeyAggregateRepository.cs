using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    internal class ProfileBlobAccessKeyAggregateRepository
        : AggregateRepository<ProfileBlobAccessKey>, IProfileBlobAccessKeyAggregateRepository
    {
        public ProfileBlobAccessKeyAggregateRepository(UnitOfWork unitOfWork)
            : base(unitOfWork)
        { }

        public async Task<ProfileBlobAccessKey?> FindAsync(
            int profileId,
            int blobId,
            ProfileBlobAccessKeyType type,
            CancellationToken ct)
        {
            return await this.FindEntityOrDefaultAsync(
                e => e.ProfileId == profileId
                    && e.BlobId == blobId
                    && e.Type == type,
                ct);
        }
    }
}
