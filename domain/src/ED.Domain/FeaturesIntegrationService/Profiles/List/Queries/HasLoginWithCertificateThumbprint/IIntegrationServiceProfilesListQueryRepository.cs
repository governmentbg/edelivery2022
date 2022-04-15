using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IIntegrationServiceProfilesListQueryRepository
    {
        Task<bool> HasLoginWithCertificateThumbprintAsync(
            string certificateThumbprint,
            CancellationToken ct);
    }
}
