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
            var message = await (
                from m in this.DbContext.Set<Message>()

                join sp in this.DbContext.Set<Profile>()
                    on m.SenderProfileId equals sp.Id

                join sl in this.DbContext.Set<Login>()
                    on m.SenderLoginId equals sl.Id

                join mr in this.DbContext.Set<MessageRecipient>()
                   on m.MessageId equals mr.MessageId

                join rp in this.DbContext.Set<Profile>()
                    on mr.ProfileId equals rp.Id

                join rl in this.DbContext.Set<Login>()
                    on mr.LoginId equals rl.Id

                join mak in this.DbContext.Set<MessageAccessKey>()
                    on m.MessageId equals mak.MessageId

                join t in this.DbContext.Set<Template>()
                    on m.TemplateId equals t.TemplateId

                join fm in this.DbContext.Set<ForwardedMessage>()
                    on m.MessageId equals fm.MessageId
                into lj4
                from fm in lj4.DefaultIfEmpty()

                where m.MessageId == messageId
                    && mr.ProfileId == profileId
                    && mak.ProfileId == profileId

                select new
                {
                    m.MessageId,
                    DateSent = m.DateSent!.Value,
                    mr.DateReceived,
                    SenderProfileId = sp.Id,
                    SenderProfileName = sp.ElectronicSubjectName,
                    SenderProfileType = sp.ProfileType,
                    SenderProfileIsReadOnly = sp.IsReadOnly,
                    SenderLoginName = sl.ElectronicSubjectName,
                    RecipientProfileId = rp.Id,
                    RecipientProfileName = rp.ElectronicSubjectName,
                    RecipientProfileType = rp.ProfileType,
                    RecipientProfileIsReadOnly = rp.IsReadOnly,
                    RecipientLoginName = rl.ElectronicSubjectName,
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
                    message.SenderProfileType,
                    message.SenderProfileIsReadOnly,
                    message.SenderLoginName),
                new GetAsRecipientVOProfile(
                    message.RecipientProfileId,
                    message.RecipientProfileName,
                    message.RecipientProfileType,
                    message.RecipientProfileIsReadOnly,
                    message.RecipientLoginName),
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
