using System;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface ICodeMessageOpenHORepository
    {
        Task<GetPdfAsRecipientVO> GetPdfAsRecipientAsync(
            Guid accessCode,
            CancellationToken ct);
    }
}
