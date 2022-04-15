using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IBlobListQueryRepository;

namespace ED.Domain
{
    partial class BlobListQueryRepository : IBlobListQueryRepository
    {
        public async Task<TableResultVO<GetInboxBlobsVO>> GetInboxBlobsAsync(
            int profileId,
            int offset,
            int limit,
            string? fileName,
            string? messageSubject,
            DateTime? fromDate,
            DateTime? toDate,
            CancellationToken ct)
        {
            Expression<Func<Blob, bool>> blobPredicate = BuildBlobPredicate(
                fileName,
                fromDate,
                toDate);

            Expression<Func<Message, bool>> messagePredicate =
                BuildMessagePredicate(messageSubject);

            TableResultVO<GetInboxBlobsVO> vos = await (
                from b in this.DbContext.Set<Blob>().Where(blobPredicate)

                join msr in this.DbContext.Set<MalwareScanResult>()
                    on b.MalwareScanResultId equals msr.Id
                    into j1
                from msr in j1.DefaultIfEmpty()

                join mb in this.DbContext.Set<MessageBlob>()
                       on b.BlobId equals mb.BlobId

                join m in this.DbContext.Set<Message>().Where(messagePredicate)
                    on mb.MessageId equals m.MessageId

                join mr in this.DbContext.Set<MessageRecipient>()
                    on m.MessageId equals mr.MessageId

                where mr.ProfileId == profileId
                    && mr.DateReceived.HasValue

                orderby m.DateCreated descending, mb.MessageBlobId

                select new GetInboxBlobsVO(
                    b.BlobId,
                    b.FileName,
                    b.Size!.Value,
                    b.CreateDate,
                    msr != null && msr.IsMalicious == false,
                    msr != null && msr.IsMalicious == true,
                    msr != null && msr.IsMalicious == null,
                    m.MessageId,
                    m.SubjectExtended))
                .ToTableResultAsync(offset, limit, ct);

            return vos;

            Expression<Func<Blob, bool>> BuildBlobPredicate(
                string? fileName,
                DateTime? fromDate,
                DateTime? toDate)
            {
                Expression<Func<Blob, bool>> predicate =
                   PredicateBuilder.True<Blob>();

                if (!string.IsNullOrEmpty(fileName))
                {
                    predicate = predicate
                        .And(e => EF.Functions.Like(e.FileName, $"%{fileName}%"));
                }

                if (fromDate.HasValue)
                {
                    predicate = predicate
                        .And(e => e.ModifyDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    predicate = predicate
                        .And(e => e.ModifyDate < toDate.Value);
                }

                return predicate;
            }

            Expression<Func<Message, bool>> BuildMessagePredicate(
                string? messageSubject)
            {
                Expression<Func<Message, bool>> predicate =
                   PredicateBuilder.True<Message>();

                if (!string.IsNullOrEmpty(messageSubject))
                {
                    predicate = predicate
                        .And(e => EF.Functions.Like(e.Subject, $"%{messageSubject}%"));
                }

                return predicate;
            }
        }
    }
}
