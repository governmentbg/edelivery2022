using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public partial interface IAdminSeosParticipantsCreateQueryRepository
    {
        [Keyless]
        public record GetRegisteredEntitiesQO(
            int Id,
            string Name,
            string Identifier,
            string ServiceUrl,
            string CertificateNumber);
    }
}
