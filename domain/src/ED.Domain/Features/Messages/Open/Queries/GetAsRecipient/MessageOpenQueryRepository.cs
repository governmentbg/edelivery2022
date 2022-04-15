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
        public async Task<GetAsRecipientVO> GetAsRecipientAsync(
            int messageId,
            int profileId,
            CancellationToken ct)
        {
            var res = await (
                from m in this.DbContext.Set<Message>()

                join p1 in this.DbContext.Set<Profile>()
                    on m.SenderProfileId equals p1.Id

                join l1 in this.DbContext.Set<Login>()
                    on m.SenderLoginId equals l1.Id

                join mr in this.DbContext.Set<MessageRecipient>()
                   on m.MessageId equals mr.MessageId

                join p2 in this.DbContext.Set<Profile>()
                    on mr.ProfileId equals p2.Id

                join l2 in this.DbContext.Set<Login>()
                    on mr.LoginId equals l2.Id

                join mak in this.DbContext.Set<MessageAccessKey>()
                    on m.MessageId equals mak.MessageId

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

                join fm in this.DbContext.Set<ForwardedMessage>()
                    on m.MessageId equals fm.MessageId
                into lj4
                from fm in lj4.DefaultIfEmpty()

                join bs in this.DbContext.Set<BlobSignature>()
                    on b.BlobId equals bs.BlobId
                into lj5
                from bs in lj5.DefaultIfEmpty()

                where m.MessageId == messageId
                    && mr.ProfileId == profileId
                    && mak.ProfileId == profileId

                select new
                {
                    MessageId = m.MessageId,
                    DateSent = m.DateSent!.Value,
                    mr.DateReceived,
                    SenderProfileId = p1.Id,
                    SenderProfileName = p1.ElectronicSubjectName,
                    SenderProfileType = p1.ProfileType,
                    SenderLoginName = l1.ElectronicSubjectName,
                    RecipientProfileId = p2.Id,
                    RecipientProfileName = p2.ElectronicSubjectName,
                    RecipientProfileType = p2.ProfileType,
                    RecipientLoginName = l2.ElectronicSubjectName,
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

            GetAsRecipientVO? vo = res
                .GroupBy(k => new
                {
                    k.MessageId,
                    k.DateSent,
                    k.DateReceived,
                    k.SenderProfileId,
                    k.SenderProfileName,
                    k.SenderProfileType,
                    k.SenderLoginName,
                    k.RecipientProfileId,
                    k.RecipientProfileName,
                    k.RecipientProfileType,
                    k.RecipientLoginName,
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
                .Select(e => new GetAsRecipientVO(
                    e.Key.MessageId,
                    e.Key.DateSent,
                    e.Key.DateReceived,
                    new GetAsRecipientVOProfile(
                        e.Key.SenderProfileId,
                        e.Key.SenderProfileName,
                        e.Key.SenderProfileType,
                        false,  // TODO:
                        e.Key.SenderLoginName),
                    new GetAsRecipientVOProfile(
                        e.Key.RecipientProfileId,
                        e.Key.RecipientProfileName,
                        e.Key.RecipientProfileType,
                        false, // todo
                        e.Key.RecipientLoginName),
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
                        .Where(b => b.BlobId.HasValue)
                        .GroupBy(b => new
                        {
                            BlobId = b.BlobId!.Value,
                            b.FileName,
                            b.Size,
                            b.DocumentRegistrationNumber,
                            Status = b.Status ?? MalwareScanResultStatus.Error,
                            b.IsMalicious
                        })
                        .Select(b => new GetAsRecipientVOBlob(
                            b.Key.BlobId,
                            b.Key.FileName,
                            b.Key.Size,
                            b.Key.DocumentRegistrationNumber,
                            b.Key.Status,
                            b.Key.IsMalicious,
                            b
                                .Where(s => s.SignatureCoversDocument.HasValue)
                                .Select(s => new GetAsRecipientVOBlobSignature(
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
