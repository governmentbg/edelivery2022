using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IIntegrationServiceMessagesOpenQueryRepository;

namespace ED.Domain
{
    partial class IntegrationServiceMessagesOpenQueryRepository : IIntegrationServiceMessagesOpenQueryRepository
    {
        public async Task<GetSentDocumentsContentVO[]> GetSentDocumentsContentAsync(
            int profileId,
            int messageId,
            CancellationToken ct)
        {
            int[] blobIds = await (
                from m in this.DbContext.Set<Message>()

                join mb in this.DbContext.Set<MessageBlob>()
                    on m.MessageId equals mb.MessageId

                where m.MessageId == messageId
                    && m.SenderProfileId == profileId

                select mb.BlobId)
                .ToArrayAsync(ct);

            var blobs = await (
                from b in this.DbContext.Set<Blob>()

                where this.DbContext.MakeIdsQuery(blobIds).Any(id => id.Id == b.BlobId)

                select new
                {
                    b.BlobId,
                    b.FileName,
                    b.Timestamp,
                    b.DocumentRegistrationNumber
                })
                .ToArrayAsync(ct);

            var blobsSignatures = await (
                from bs in this.DbContext.Set<BlobSignature>()

                where this.DbContext.MakeIdsQuery(blobIds).Any(id => id.Id == bs.BlobId)

                select new
                {
                    bs.BlobId,
                    bs.X509Certificate2DER,
                    bs.CoversDocument,
                    bs.CoversPriorRevision,
                    bs.IsTimestamp,
                    bs.SignDate,
                    bs.ValidAtTimeOfSigning,
                    bs.Issuer,
                    bs.Subject,
                    bs.SerialNumber,
                    bs.Version,
                    bs.ValidFrom,
                    bs.ValidTo,
                })
                .ToArrayAsync(ct);

            GetSentDocumentsContentVO[] result =
                new GetSentDocumentsContentVO[blobIds.Length];

            for (int i = 0; i < blobIds.Length; i++)
            {
                var matchBlob = blobs.First(e => e.BlobId == blobIds[i]);

                result[i] = new(
                    matchBlob.BlobId,
                    matchBlob.FileName,
                    matchBlob.Timestamp,
                    matchBlob.DocumentRegistrationNumber,
                    blobsSignatures
                        .Where(e => e.BlobId == blobIds[i])
                        .Select(s => new GetSentDocumentsContentVOSignature(
                            s.X509Certificate2DER ?? Array.Empty<byte>(),
                            s.CoversDocument,
                            s.CoversPriorRevision,
                            s.IsTimestamp,
                            s.SignDate,
                            s.ValidAtTimeOfSigning,
                            s.Issuer,
                            s.Subject,
                            s.SerialNumber,
                            s.Version,
                            s.ValidFrom,
                            s.ValidTo))
                        .ToArray());
            }

            return result;
        }
    }
}
