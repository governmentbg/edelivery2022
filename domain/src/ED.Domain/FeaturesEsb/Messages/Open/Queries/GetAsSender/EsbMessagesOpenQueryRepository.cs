using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IEsbMessagesOpenQueryRepository;

namespace ED.Domain
{
    partial class EsbMessagesOpenQueryRepository : IEsbMessagesOpenQueryRepository
    {
        public async Task<GetAsSenderVO> GetAsSenderAsync(
            int messageId,
            CancellationToken ct)
        {
            var message = await (
                from m in this.DbContext.Set<Message>()

                join mak in this.DbContext.Set<MessageAccessKey>()
                    on new { messageId = m.MessageId, profileId = m.SenderProfileId } equals new { messageId = mak.MessageId, profileId = mak.ProfileId }

                join fm in this.DbContext.Set<ForwardedMessage>()
                    on m.MessageId equals fm.MessageId
                    into lj5
                from fm in lj5.DefaultIfEmpty()

                where m.MessageId == messageId

                select new
                {
                    m.MessageId,
                    DateSent = m.DateSent!.Value,
                    m.RecipientsAsText,
                    m.TemplateId,
                    m.Subject,
                    m.Orn,
                    m.ReferencedOrn,
                    m.AdditionalIdentifier,
                    m.Body,
                    mak.ProfileKeyId,
                    mak.EncryptedKey,
                    m.IV,
                    ForwardedMessageId = (int?)fm.ForwardedMessageId,
                })
                .FirstOrDefaultAsync(ct);

            GetAsSenderVOProfile[] recipients = await (
                from mr in this.DbContext.Set<MessageRecipient>()

                join p in this.DbContext.Set<Profile>()
                    on mr.ProfileId equals p.Id

                where mr.MessageId == messageId

                select new GetAsSenderVOProfile(
                    mr.ProfileId,
                    p.ElectronicSubjectName,
                    mr.DateReceived))
                .ToArrayAsync(ct);

            var blobsAndSignatures = await (
                from mb in this.DbContext.Set<MessageBlob>()

                join b in this.DbContext.Set<Blob>()
                    on mb.BlobId equals b.BlobId

                join msr in this.DbContext.Set<MalwareScanResult>()
                    on b.MalwareScanResultId equals msr.Id
                    into lj3
                from msr in lj3.DefaultIfEmpty()

                join bs in this.DbContext.Set<BlobSignature>()
                    on b.BlobId equals bs.BlobId
                    into lj4
                from bs in lj4.DefaultIfEmpty()

                where mb.MessageId == messageId

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

            GetAsSenderVOBlob[] blobs = blobsAndSignatures
                .GroupBy(e => e.BlobId)
                .Select(e => new GetAsSenderVOBlob(
                    e.Key,
                    e.First().FileName,
                    e.First().Size,
                    e.First().DocumentRegistrationNumber,
                    e.First().IsMalicious,
                    e.First().Hash,
                    e.First().HashAlgorithm,
                    e
                        .Where(s => s.SignatureCoversDocument.HasValue)
                        .Select(s => new GetAsSenderVOBlobSignature(
                            s.SignatureCoversDocument!.Value,
                            s.SignatureSignDate!.Value,
                            s.SignatureIsTimestamp!.Value,
                            s.SignatureValidAtTimeOfSigning!.Value,
                            s.SignatureIssuer,
                            s.SignatureSubject,
                            s.SignatureValidFrom!.Value,
                            s.SignatureValidTo!.Value))
                        .ToArray()
                    ))
                .ToArray();

            GetAsSenderVO vo = new(
                message.MessageId,
                message.DateSent,
                recipients,
                message.Subject,
                message.Orn,
                message.ReferencedOrn,
                message.AdditionalIdentifier,
                message.TemplateId!.Value,
                message.Body,
                message.ProfileKeyId,
                message.EncryptedKey,
                message.IV,
                blobs,
                message.ForwardedMessageId);

            return vo;
        }
    }
}
