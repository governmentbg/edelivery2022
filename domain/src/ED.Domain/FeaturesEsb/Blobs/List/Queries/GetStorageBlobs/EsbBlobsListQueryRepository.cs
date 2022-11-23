using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IEsbBlobsListQueryRepository;

namespace ED.Domain
{
    partial class EsbBlobsListQueryRepository : IEsbBlobsListQueryRepository
    {
        public async Task<TableResultVO<GetStorageBlobsVO>> GetStorageBlobsAsync(
            int profileId,
            int? offset,
            int? limit,
            CancellationToken ct)
        {
            IQueryable<int> countQuery =
                from b in this.DbContext.Set<Blob>()

                join pbak in this.DbContext.Set<ProfileBlobAccessKey>()
                    on b.BlobId equals pbak.BlobId

                where pbak.ProfileId == profileId
                    && pbak.Type == ProfileBlobAccessKeyType.Storage

                select b.BlobId;

            int count = await countQuery.CountAsync(ct);

            if (count == 0)
            {
                return TableResultVO.Empty<GetStorageBlobsVO>();
            }

            IQueryable<Blob> blobs =(
                from b in this.DbContext.Set<Blob>()

                join pbak in this.DbContext.Set<ProfileBlobAccessKey>()
                    on b.BlobId equals pbak.BlobId

                where pbak.ProfileId == profileId
                    && pbak.Type == ProfileBlobAccessKeyType.Storage

                select b)
                .WithOffsetAndLimit(offset, limit);

            var blobsAndSignatures = await (
                from b in blobs

                join msr in this.DbContext.Set<MalwareScanResult>()
                    on b.MalwareScanResultId equals msr.Id
                    into lj3
                from msr in lj3.DefaultIfEmpty()

                join bs in this.DbContext.Set<BlobSignature>()
                    on b.BlobId equals bs.BlobId
                    into lj4
                from bs in lj4.DefaultIfEmpty()

                select new
                {
                    b.BlobId,
                    b.FileName,
                    b.Size,
                    b.DocumentRegistrationNumber,
                    msr.IsMalicious,
                    b.Hash,
                    b.HashAlgorithm,
                    SignatureCoversDocument = (bool?)bs.CoversDocument,
                    SignatureSignDate = (DateTime?)bs.SignDate,
                    SignatureIsTimestamp = (bool?)bs.IsTimestamp,
                    SignatureValidAtTimeOfSigning = (bool?)bs.ValidAtTimeOfSigning,
                    SignatureIssuer = (string?)bs.Issuer,
                    SignatureSubject = (string?)bs.Subject,
                    SignatureValidFrom = (DateTime?)bs.ValidFrom,
                    SignatureValidTo = (DateTime?)bs.ValidTo,
                })
                .ToArrayAsync(ct);

            GetStorageBlobsVO[] vos = blobsAndSignatures
                .GroupBy(e => e.BlobId)
                .Select(e => new GetStorageBlobsVO(
                    e.Key,
                    e.First().FileName,
                    e.First().Size,
                    e.First().DocumentRegistrationNumber,
                    e.First().IsMalicious,
                    e.First().Hash,
                    e.First().HashAlgorithm,
                    e
                        .Where(s => s.SignatureCoversDocument.HasValue)
                        .Select(s => new GetStorageBlobsVOSignature(
                            s.SignatureCoversDocument!.Value,
                            s.SignatureSignDate!.Value,
                            s.SignatureIsTimestamp!.Value,
                            s.SignatureValidAtTimeOfSigning!.Value,
                            s.SignatureIssuer,
                            s.SignatureSubject,
                            s.SignatureValidFrom!.Value,
                            s.SignatureValidTo!.Value))
                        .ToArray()))
                .ToArray();

            return new TableResultVO<GetStorageBlobsVO>(vos, count);
        }
    }
}
