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
        public async Task<GetSentDocumentContentVO?> GetSentDocumentContentAsync(
            int profileId,
            int blobId,
            CancellationToken ct)
        {
            var blob = await (
                from m in this.DbContext.Set<Message>()

                join mb in this.DbContext.Set<MessageBlob>()
                    on m.MessageId equals mb.MessageId

                join b in this.DbContext.Set<Blob>()
                    on mb.BlobId equals b.BlobId

                where m.SenderProfileId == profileId
                   && mb.BlobId == blobId

                select new
                {
                    m.MessageId,
                    b.BlobId,
                    b.FileName,
                    b.Timestamp,
                    b.DocumentRegistrationNumber
                })
                .SingleAsync(ct);

            var blobSignatures = await (
                from bs in this.DbContext.Set<BlobSignature>()

                where bs.BlobId == blob.BlobId

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
                    blob.BlobId,
                    blob.MessageId,
                    ct);

            return new GetSentDocumentContentVO(
                blob.BlobId,
                blob.FileName,
                downloadBlob.Content,
                blob.Timestamp,
                blob.DocumentRegistrationNumber,
                blobSignatures
                    .Select(s => new GetSentDocumentContentVOSignature(
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
