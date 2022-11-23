using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        Task<FindRecipientLegalEntityVO?> FindRecipientLegalEntityAsync(
            string identifier,
            int templateId,
            CancellationToken ct);
    }
}
