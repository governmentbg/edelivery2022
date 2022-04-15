using System;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface ICodeMessageOpenQueryRepository
    {
        Task<GetPdfAsRecipientVO> GetPdfAsRecipientAsync(
            Guid accessCode,
            CancellationToken ct);
    }
}
