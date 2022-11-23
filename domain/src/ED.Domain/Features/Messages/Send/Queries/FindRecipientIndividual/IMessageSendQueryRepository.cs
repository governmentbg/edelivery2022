using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        Task<FindRecipientIndividualVO?> FindRecipientIndividualAsync(
            string firstName,
            string lastName,
            string identifier,
            int templateId,
            CancellationToken ct);
    }
}
