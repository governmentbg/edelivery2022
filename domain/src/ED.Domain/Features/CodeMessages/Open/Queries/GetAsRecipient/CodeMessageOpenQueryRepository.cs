using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.ICodeMessageOpenQueryRepository;

namespace ED.Domain
{
    partial class CodeMessageOpenQueryRepository : ICodeMessageOpenQueryRepository
    {
        // TODO: redo with message code
        public async Task<GetAsRecipientVO> GetAsRecipientAsync(
            int messageId,
            CancellationToken ct)
        {
            var res = await (
                from m in this.DbContext.Set<Message>()

                join mac in this.DbContext.Set<MessagesAccessCode>()
                    on m.MessageId equals mac.MessageId

                join p1 in this.DbContext.Set<Profile>()
                    on m.SenderProfileId equals p1.Id

                join l1 in this.DbContext.Set<Login>()
                    on m.SenderLoginId equals l1.Id

                join mr in this.DbContext.Set<MessageRecipient>()
                   on m.MessageId equals mr.MessageId

                // TODO: rely on the fact that code messages could not be forwared
                join p2 in this.DbContext.Set<Profile>()
                    on mr.ProfileId equals p2.Id

                join l2 in this.DbContext.Set<Login>()
                    on mr.LoginId equals l2.Id
                    into lj1
                from l2 in lj1.DefaultIfEmpty()

                join mak in this.DbContext.Set<MessageAccessKey>()
                    on new { m.MessageId, mr.ProfileId } equals new { mak.MessageId, mak.ProfileId }

                join t in this.DbContext.Set<Template>()
                    on m.TemplateId equals t.TemplateId

                join mb in this.DbContext.Set<MessageBlob>()
                    on m.MessageId equals mb.MessageId
                into lj2
                from mb in lj2.DefaultIfEmpty()

                join b in this.DbContext.Set<Blob>()
                    on mb.BlobId equals b.BlobId
                into lj3
                from b in lj3.DefaultIfEmpty()

                join msr in this.DbContext.Set<MalwareScanResult>()
                    on b.MalwareScanResultId equals msr.Id
                into lj4
                from msr in lj4.DefaultIfEmpty()

                join bs in this.DbContext.Set<BlobSignature>()
                    on b.BlobId equals bs.BlobId
                into lj5
                from bs in lj5.DefaultIfEmpty()

                where m.MessageId == messageId

                select new
                {
                    m.MessageId,
                    mac.AccessCode,
                    DateSent = m.DateSent!.Value,
                    mr.DateReceived,
                    SenderProfileId = p1.Id,
                    SenderProfileName = p1.ElectronicSubjectName,
                    SenderProfileType = p1.ProfileType,
                    SenderLoginName = l1.ElectronicSubjectName,
                    RecipientProfileId = p2.Id,
                    RecipientProfileName = p2.ElectronicSubjectName,
                    RecipientProfileType = p2.ProfileType,
                    RecipientLoginName = l2 != null ? l2.ElectronicSubjectName : string.Empty,
                    t.TemplateId,
                    m.Subject,
                    m.Orn,
                    m.ReferencedOrn,
                    m.AdditionalIdentifier,
                    m.Body,
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
                    k.AccessCode,
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
                    k.ProfileKeyId,
                    k.TemplateName,
                })
                .Select(e => new GetAsRecipientVO(
                    e.Key.MessageId,
                    e.Key.DateSent,
                    e.Key.DateReceived,
                    new GetAsRecipientVOProfile(
                        e.Key.SenderProfileId,
                        e.Key.SenderProfileName,
                        e.Key.SenderLoginName),
                    new GetAsRecipientVOProfile(
                        e.Key.RecipientProfileId,
                        e.Key.RecipientProfileName,
                        e.Key.RecipientLoginName),
                    e.Key.TemplateId,
                    e.Key.Subject,
                    e.Key.Orn,
                    e.Key.ReferencedOrn,
                    e.Key.AdditionalIdentifier,
                    e.First().Body,
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
                    e.Key.AccessCode.ToString()))
                .Single();

            return vo;
        }
    }
}
