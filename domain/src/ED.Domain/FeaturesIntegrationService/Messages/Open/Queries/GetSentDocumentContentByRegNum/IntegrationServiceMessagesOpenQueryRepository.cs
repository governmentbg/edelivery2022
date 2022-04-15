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
        public async Task<GetSentDocumentContentByRegNumVO?> GetSentDocumentContentByRegNumAsync(
            int profileId,
            string documentRegistrationNumber,
            CancellationToken ct)
        {
            var messageBlobIds = await (
                from m in this.DbContext.Set<Message>()

                join mb in this.DbContext.Set<MessageBlob>()
                    on m.MessageId equals mb.MessageId

                join b in this.DbContext.Set<Blob>()
                    on mb.BlobId equals b.BlobId

                where m.SenderProfileId == profileId
                    && EF.Functions.Like(b.DocumentRegistrationNumber, documentRegistrationNumber)

                select new { m.MessageId, b.BlobId })
                .FirstOrDefaultAsync(ct);

            if (messageBlobIds == null)
            {
                return null;
            }

            var blob = await (
                from b1 in this.DbContext.Set<Blob>()

                where b1.BlobId == messageBlobIds.BlobId

                select new
                {
                    b1.BlobId,
                    b1.FileName,
                    b1.Timestamp,
                    b1.DocumentRegistrationNumber
                })
                .SingleAsync(ct);

            var blobSignatures = await (
                from bs in this.DbContext.Set<BlobSignature>()

                where bs.BlobId == messageBlobIds.BlobId

                select new
                {
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

            BlobsServiceClient.DownloadBlobToArrayVO downloadBlob =
                await this.blobsServiceClient.DownloadMessageBlobToArrayAsync(
                    profileId,
                    messageBlobIds.BlobId,
                    messageBlobIds.MessageId,
                    ct);

            return new GetSentDocumentContentByRegNumVO(
                blob.BlobId,
                blob.FileName,
                downloadBlob.Content,
                blob.Timestamp,
                blob.DocumentRegistrationNumber,
                blobSignatures
                    .Select(s => new GetSentDocumentContentByRegNumVOSignature(
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
                    .ToArray()
                );
        }
    }
}
