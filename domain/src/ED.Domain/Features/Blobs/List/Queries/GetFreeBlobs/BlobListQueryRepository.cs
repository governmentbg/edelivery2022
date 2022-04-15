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
        public async Task<TableResultVO<GetFreeBlobsVO>> GetFreeBlobsAsync(
            int profileId,
            int offset,
            int limit,
            string? fileName,
            string? author,
            DateTime? fromDate,
            DateTime? toDate,
            CancellationToken ct)
        {
            Expression<Func<Blob, bool>> blobPredicate = BuildBlobPredicate(
                fileName,
                fromDate,
                toDate);

            Expression<Func<Login, bool>> loginPredicate =
                BuildLoginPredicate(author);

            TableResultVO<GetFreeBlobsVO> vos = await (
                from b in this.DbContext.Set<Blob>().Where(blobPredicate)
                join pb in this.DbContext.Set<ProfileBlobAccessKey>()
                    on b.BlobId equals pb.BlobId

                join msr in this.DbContext.Set<MalwareScanResult>()
                    on b.MalwareScanResultId equals msr.Id
                    into j1
                from msr in j1.DefaultIfEmpty()

                join l in this.DbContext.Set<Login>().Where(loginPredicate)
                    on pb.CreatedByLoginId equals l.Id

                where pb.ProfileId == profileId
                    && pb.Type == ProfileBlobAccessKeyType.Storage

                orderby b.CreateDate descending

                select new GetFreeBlobsVO(
                    b.BlobId,
                    b.FileName,
                    b.Size!.Value,
                    b.CreateDate,
                    msr != null && msr.IsMalicious == false,
                    msr != null && msr.IsMalicious == true,
                    msr != null && msr.IsMalicious == null))
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

            Expression<Func<Login, bool>> BuildLoginPredicate(string? author)
            {
                Expression<Func<Login, bool>> predicate =
                   PredicateBuilder.True<Login>();

                if (!string.IsNullOrEmpty(author))
                {
                    predicate = predicate
                        .And(e => EF.Functions.Like(e.ElectronicSubjectName, $"%{author}%"));
                }

                return predicate;
            }
        }
    }
}
