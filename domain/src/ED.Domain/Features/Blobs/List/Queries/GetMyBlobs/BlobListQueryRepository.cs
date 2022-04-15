using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using static ED.Domain.IBlobListQueryRepository;

namespace ED.Domain
{
    partial class BlobListQueryRepository : IBlobListQueryRepository
    {
        public async Task<TableResultVO<GetMyBlobsVO>> GetMyBlobsAsync(
            int profileId,
            long? maxFileSize,
            string[]? allowedFileTypes,
            int offset,
            int limit,
            CancellationToken ct)
        {
            Expression<Func<Blob, bool>> blobPredicate = BuildBlobPredicate(
                maxFileSize,
                allowedFileTypes);

            return await
                (from b in this.DbContext.Set<Blob>().Where(blobPredicate)
                 join pb in this.DbContext.Set<ProfileBlobAccessKey>()
                     on b.BlobId equals pb.BlobId

                 join msr in this.DbContext.Set<MalwareScanResult>()
                     on b.MalwareScanResultId equals msr.Id
                     into j1
                 from msr in j1.DefaultIfEmpty()

                 where pb.ProfileId == profileId
                       && pb.Type == ProfileBlobAccessKeyType.Storage

                 orderby b.CreateDate descending

                 select new GetMyBlobsVO(
                     b.BlobId,
                     b.FileName,
                     b.HashAlgorithm!,
                     b.Hash!,
                     b.Size!.Value,
                     b.CreateDate,
                     msr != null && msr.IsMalicious == false,
                     msr != null && msr.IsMalicious == true,
                     msr != null && msr.IsMalicious == null)
                 )
                .ToTableResultAsync(offset, limit, ct);

            Expression<Func<Blob, bool>> BuildBlobPredicate(
                long? maxFileSize,
                string[]? allowedFileTypes)
            {
                Expression<Func<Blob, bool>> predicate =
                    PredicateBuilder.True<Blob>();

                if (maxFileSize.HasValue && maxFileSize > 0)
                {
                    predicate = predicate
                        .And(e => e.Size <= maxFileSize);
                }

                if (allowedFileTypes is { Length: > 0 })
                {
                    predicate = predicate
                        .And(e => this.DbContext.MakeIdsQuery(allowedFileTypes)
                            .Any(x => x.Id == e.FileExtension));
                }

                return predicate;
            }
        }
    }
}
