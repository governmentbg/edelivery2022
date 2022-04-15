using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public interface IProfilesService
    {
        public record ProfileKeyVO(
            int ProfileId,
            int ProfileKeyId,
            string Provider,
            string KeyName,
            string OaepPadding);
        Task<ProfileKeyVO> GetOrCreateProfileKeyAndSaveAsync(
            int profileId,
            CancellationToken ct);
        Task<ProfileKeyVO> GetProfileKeyAsync(
            int profileKeyId,
            CancellationToken ct);
    }
}
