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
            var res = await (
                from m in this.DbContext.Set<Message>()

                join p1 in this.DbContext.Set<Profile>()
                    on m.SenderProfileId equals p1.Id

                join mak in this.DbContext.Set<MessageAccessKey>()
                    on new { messageId = m.MessageId, profileId = m.SenderProfileId } equals new { messageId = mak.MessageId, profileId = mak.ProfileId }

                join t in this.DbContext.Set<Template>()
                    on m.TemplateId equals t.TemplateId

                join mb in this.DbContext.Set<MessageBlob>()
                    on m.MessageId equals mb.MessageId
                into lj1
                from mb in lj1.DefaultIfEmpty()

                join b in this.DbContext.Set<Blob>()
                    on mb.BlobId equals b.BlobId
                into lj2
                from b in lj2.DefaultIfEmpty()

                join msr in this.DbContext.Set<MalwareScanResult>()
                    on b.MalwareScanResultId equals msr.Id
                into lj3
                from msr in lj3.DefaultIfEmpty()

                join bs in this.DbContext.Set<BlobSignature>()
                   on b.BlobId equals bs.BlobId
               into lj4
                from bs in lj4.DefaultIfEmpty()

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
                    m.Orn,
                    m.ReferencedOrn,
                    m.AdditionalIdentifier,
                    m.Body,
                    m.ForwardStatusId,
                    mak.ProfileKeyId,
                    mak.EncryptedKey,
                    m.IV,
                    TemplateName = t.Name,
                    BlobId = (int?)b.BlobId,
                    FileName = (string?)b.FileName,
                    b.Size,
                    b.DocumentRegistrationNumber,
                    msr.Status,
                    msr.IsMalicious,
                    ForwardedMessageId = (int?)fm.ForwardedMessageId,
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

            GetAsSenderVO? vo = res
                .GroupBy(k => new
                {
                    k.MessageId,
                    k.DateSent,
                    k.SenderProfileId,
                    k.SenderProfileName,
                    k.RecipientsAsText,
                    k.TemplateId,
                    k.Subject,
                    k.Orn,
                    k.ReferencedOrn,
                    k.AdditionalIdentifier,
                    k.ForwardStatusId,
                    k.ProfileKeyId,
                    k.TemplateName,
                    k.ForwardedMessageId
                })
                .Select(e => new GetAsSenderVO(
                    e.Key.MessageId,
                    e.Key.DateSent,
                    new GetAsSenderVOProfile(
                        e.Key.SenderProfileId,
                        e.Key.SenderProfileName),
                    e.Key.RecipientsAsText,
                    e.Key.TemplateId,
                    e.Key.Subject,
                    e.Key.Orn,
                    e.Key.ReferencedOrn,
                    e.Key.AdditionalIdentifier,
                    e.First().Body,
                    e.Key.ForwardStatusId,
                    e.Key.ProfileKeyId,
                    e.First().EncryptedKey,
                    e.First().IV,
                    e.Key.TemplateName,
                    e
                        .Where(e => e.BlobId.HasValue)
                        .GroupBy(b => new
                        {
                            BlobId = b.BlobId!.Value,
                            b.FileName,
                            b.Size,
                            b.DocumentRegistrationNumber,
                            Status = b.Status ?? MalwareScanResultStatus.Error,
                            b.IsMalicious
                        })
                        .Select(b => new GetAsSenderVOBlob(
                            b.Key.BlobId,
                            b.Key.FileName,
                            b.Key.Size,
                            b.Key.DocumentRegistrationNumber,
                            b.Key.Status,
                            b.Key.IsMalicious,
                            b
                                .Where(s => s.SignatureCoversDocument.HasValue)
                                .Select(s => new GetAsSenderVOBlobSignature(
                                    s.SignatureCoversDocument!.Value,
                                    s.SignatureSignDate!.Value,
                                    s.SignatureIsTimestamp!.Value,
                                    s.SignatureValidAtTimeOfSigning!.Value,
                                    s.SignatureIssuer!,
                                    s.SignatureSubject!,
                                    s.SignatureValidFrom!.Value,
                                    s.SignatureValidTo!.Value))
                                .ToArray()))
                        .ToArray(),
                    e.Key.ForwardedMessageId))
                .Single();

            return vo;
        }
    }
}
