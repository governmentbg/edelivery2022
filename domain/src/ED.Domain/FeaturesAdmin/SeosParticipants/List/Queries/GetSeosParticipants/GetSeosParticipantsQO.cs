using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public partial interface IAdminSeosParticipantsListQueryRepository
    {
        [Keyless]
        public record GetSeosParticipantsQO(
            int Id,
            string Identifier,
            string Name,
            string? Email,
            string? Phone,
            string? ServiceUrl,
            string CertificateNumber);
    }
}
