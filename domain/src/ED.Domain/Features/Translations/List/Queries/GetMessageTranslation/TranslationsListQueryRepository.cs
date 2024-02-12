using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.ITranslationsListQueryRepository;

namespace ED.Domain
{
    partial class TranslationsListQueryRepository : ITranslationsListQueryRepository
    {
        public async Task<GetMessageTranslationVO> GetMessageTranslationAsync(
            int messageTranslationId,
            CancellationToken ct)
        {
            var translation = await (
                from mt in this.DbContext.Set<MessageTranslation>()

                join m in this.DbContext.Set<Message>()
                    on mt.MessageId equals m.MessageId

                where mt.MessageTranslationId == messageTranslationId
                    && mt.ArchiveDate == null

                select new
                {
                    mt.MessageTranslationId,
                    m.MessageId,
                    m.Subject,
                    mt.SourceLanguage,
                    mt.TargetLanguage,
                    mt.CreateDate,
                    mt.ModifyDate,
                })
            .FirstAsync(ct);

            var requests = await (
                from mtr in this.DbContext.Set<MessageTranslationRequest>()

                join b1 in this.DbContext.Set<Blob>()
                    on mtr.SourceBlobId equals b1.BlobId
                    into lj1
                from b1 in lj1.DefaultIfEmpty()

                join b2 in this.DbContext.Set<Blob>()
                    on mtr.TargetBlobId equals b2.BlobId
                    into lj2
                from b2 in lj2.DefaultIfEmpty()

                where mtr.MessageTranslationId == translation.MessageTranslationId

                orderby mtr.MessageTranslationRequestId

                select new
                {
                    mtr.RequestId,
                    mtr.SourceBlobId,
                    SourceBlobFileName = (string?)b1.FileName,
                    mtr.TargetBlobId,
                    TargetBlobFileName = (string?)b2.FileName,
                    mtr.Status,
                    mtr.ErrorMessage
                })
            .ToArrayAsync(ct);

            GetMessageTranslationVO vo = new(
                translation.MessageTranslationId,
                translation.MessageId,
                translation.Subject,
                translation.SourceLanguage,
                translation.TargetLanguage,
                translation.CreateDate,
                translation.ModifyDate,
                requests
                    .Select(e => new GetMessageTranslationVORequest(
                        e.RequestId,
                        e.SourceBlobId,
                        e.SourceBlobFileName,
                        e.TargetBlobId,
                        e.TargetBlobFileName,
                        e.Status,
                        e.ErrorMessage))
                    .ToArray());

            return vo;
        }
    }
}
