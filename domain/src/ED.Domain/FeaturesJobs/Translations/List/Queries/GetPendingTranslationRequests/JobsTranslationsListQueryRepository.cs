using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IJobsTranslationsListQueryRepository;

namespace ED.Domain
{
    partial class JobsTranslationsListQueryRepository : IJobsTranslationsListQueryRepository
    {
        public async Task<GetPendingTranslationRequestsVO[]> GetPendingTranslationRequestsAsync(
            int messageTranslationId,
            CancellationToken ct)
        {
            GetPendingTranslationRequestsVO[] vos = await (
                from mt in this.DbContext.Set<MessageTranslation>()

                join mtr in this.DbContext.Set<MessageTranslationRequest>()
                    on mt.MessageTranslationId equals mtr.MessageTranslationId

                join b in this.DbContext.Set<Blob>()
                    on mtr.SourceBlobId equals b.BlobId
                    into lj1
                from b in lj1.DefaultIfEmpty()

                where mtr.MessageTranslationId == messageTranslationId
                    && mtr.Status == MessageTranslationRequestStatus.Pending
                    && mtr.RequestId == null
                    && mt.ArchiveDate == null

                select new GetPendingTranslationRequestsVO(
                    mt.MessageId,
                    mt.ProfileId,
                    mt.SourceLanguage,
                    mt.TargetLanguage,
                    mtr.SourceBlobId,
                    b.FileName,
                    b.Size))
                .ToArrayAsync(ct);

            return vos;
        }
    }
}
