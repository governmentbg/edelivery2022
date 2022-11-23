using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IMessageOpenQueryRepository;

namespace ED.Domain
{
    partial class MessageOpenQueryRepository : IMessageOpenQueryRepository
    {
        public async Task<GetAsSenderVO> GetAsSenderAsync(
            int messageId,
            CancellationToken ct)
        {
            var message = await (
                from m in this.DbContext.Set<Message>()

                join p1 in this.DbContext.Set<Profile>()
                    on m.SenderProfileId equals p1.Id

                join mak in this.DbContext.Set<MessageAccessKey>()
                    on new { messageId = m.MessageId, profileId = m.SenderProfileId } equals new { messageId = mak.MessageId, profileId = mak.ProfileId }

                join t in this.DbContext.Set<Template>()
                    on m.TemplateId equals t.TemplateId

                join fm in this.DbContext.Set<ForwardedMessage>()
                    on m.MessageId equals fm.MessageId
                into lj5
                from fm in lj5.DefaultIfEmpty()

                where m.MessageId == messageId

                select new
                {
                    m.MessageId,
                    DateSent = m.DateSent!.Value,
                    SenderProfileId = p1.Id,
                    SenderProfileName = p1.ElectronicSubjectName,
                    m.RecipientsAsText,
                    t.TemplateId,
                    m.Subject,
                    m.Rnu,
                    m.Body,
                    m.ForwardStatusId,
                    mak.ProfileKeyId,
                    mak.EncryptedKey,
                    m.IV,
                    TemplateName = t.Name,
                    ForwardedMessageId = (int?)fm.ForwardedMessageId,
                })
                .FirstAsync(ct);

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
                    msr.Status,
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
                    e.First().Hash,
                    e.First().Size,
                    e.First().DocumentRegistrationNumber,
                    e.First().Status ?? MalwareScanResultStatus.Error,
                    e.First().IsMalicious,
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
                        .ToArray()))
                .ToArray();

            GetAsSenderVO vo = new(
                message.MessageId,
                message.DateSent,
                new GetAsSenderVOProfile(
                    message.SenderProfileId,
                    message.SenderProfileName),
                message.RecipientsAsText,
                message.TemplateId,
                message.Subject,
                message.Rnu,
                message.Body,
                message.ForwardStatusId,
                message.ProfileKeyId,
                message.EncryptedKey,
                message.IV,
                message.TemplateName,
                blobs,
                message.ForwardedMessageId);

            return vo;
        }
    }
}
