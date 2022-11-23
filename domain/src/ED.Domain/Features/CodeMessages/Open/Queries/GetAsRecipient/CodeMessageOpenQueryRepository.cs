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
        public async Task<GetAsRecipientVO> GetAsRecipientAsync(
            int messageId,
            CancellationToken ct)
        {
            var message = await (
                from m in this.DbContext.Set<Message>()

                join mac in this.DbContext.Set<MessagesAccessCode>()
                    on m.MessageId equals mac.MessageId

                join p1 in this.DbContext.Set<Profile>()
                    on m.SenderProfileId equals p1.Id

                join l1 in this.DbContext.Set<Login>()
                    on m.SenderLoginId equals l1.Id

                join mr in this.DbContext.Set<MessageRecipient>() // only 1 recipient
                   on m.MessageId equals mr.MessageId

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
                    m.Rnu,
                    m.Body,
                    mak.ProfileKeyId,
                    mak.EncryptedKey,
                    m.IV,
                    TemplateName = t.Name,
                })
                .FirstAsync(ct);

            var blobsAndSignatures = await (
                from mb in this.DbContext.Set<MessageBlob>()

                join b in this.DbContext.Set<Blob>()
                    on mb.BlobId equals b.BlobId

                join msr in this.DbContext.Set<MalwareScanResult>()
                    on b.MalwareScanResultId equals msr.Id
                into lj4
                from msr in lj4.DefaultIfEmpty()

                join bs in this.DbContext.Set<BlobSignature>()
                    on b.BlobId equals bs.BlobId
                into lj5
                from bs in lj5.DefaultIfEmpty()

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

            GetAsRecipientVOBlob[] blobs = blobsAndSignatures
                .GroupBy(e => e.BlobId)
                .Select(e => new GetAsRecipientVOBlob(
                    e.Key,
                    e.First().FileName,
                    e.First().Hash,
                    e.First().Size,
                    e.First().DocumentRegistrationNumber,
                    e.First().Status ?? MalwareScanResultStatus.Error,
                    e.First().IsMalicious,
                    e
                        .Where(s => s.SignatureCoversDocument.HasValue)
                        .Select(s => new GetAsRecipientVOBlobSignature(
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

            GetAsRecipientVO vo = new(
                message.MessageId,
                message.DateSent,
                message.DateReceived,
                new GetAsRecipientVOProfile(
                    message.SenderProfileId,
                    message.SenderProfileName,
                    message.SenderLoginName),
                new GetAsRecipientVOProfile(
                    message.RecipientProfileId,
                    message.RecipientProfileName,
                    message.RecipientLoginName),
                message.TemplateId,
                message.Subject,
                message.Rnu,
                message.Body,
                message.ProfileKeyId,
                message.EncryptedKey,
                message.IV,
                message.TemplateName,
                blobs,
                message.AccessCode.ToString());

            return vo;
        }
    }
}
