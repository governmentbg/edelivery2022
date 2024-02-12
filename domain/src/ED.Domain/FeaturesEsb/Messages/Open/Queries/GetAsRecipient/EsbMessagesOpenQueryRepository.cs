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
        public async Task<GetAsRecipientVO> GetAsRecipientAsync(
            int messageId,
            int profileId,
            CancellationToken ct)
        {
            var message = await (
                from m in this.DbContext.Set<Message>()

                join sp in this.DbContext.Set<Profile>()
                    on m.SenderProfileId equals sp.Id

                join mr in this.DbContext.Set<MessageRecipient>()
                    on m.MessageId equals mr.MessageId

                join rp in this.DbContext.Set<Profile>()
                    on mr.ProfileId equals rp.Id

                join sl in this.DbContext.Set<Login>()
                    on mr.LoginId equals sl.Id
                    into lj1
                from sl in lj1.DefaultIfEmpty()

                join mak in this.DbContext.Set<MessageAccessKey>()
                    on m.MessageId equals mak.MessageId

                join fm in this.DbContext.Set<ForwardedMessage>()
                    on m.MessageId equals fm.MessageId
                    into lj5
                from fm in lj5.DefaultIfEmpty()

                where m.MessageId == messageId
                    && mr.ProfileId == profileId
                    && mak.ProfileId == profileId

                select new
                {
                    m.MessageId,
                    DateSent = m.DateSent!.Value,
                    SenderProfileId = sp.Id,
                    SenderProfileName = sp.ElectronicSubjectName,
                    mr.DateReceived,
                    RecipientProfileId = rp.Id,
                    RecipientProfileName = rp.ElectronicSubjectName,
                    RecipientLoginId = sl != null ? (int?)sl.Id : null,
                    RecipientLoginName = sl != null ? (string?)sl.ElectronicSubjectName : null,
                    m.TemplateId,
                    m.Subject,
                    m.Rnu,
                    m.Body,
                    mak.ProfileKeyId,
                    mak.EncryptedKey,
                    m.IV,
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
                    e.First().Size,
                    e.First().DocumentRegistrationNumber,
                    e.First().IsMalicious,
                    e.First().Hash,
                    e.First().HashAlgorithm,
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
                        .ToArray()
                    ))
                .ToArray();

            GetAsRecipientVO vo = new(
                message.MessageId,
                message.DateSent,
                new GetAsRecipientVOProfileSender(
                    message.SenderProfileId,
                    message.SenderProfileName),
                message.DateReceived,
                new GetAsRecipientVOProfileRecipient(
                    message.RecipientProfileId,
                    message.RecipientProfileName),
                message.DateReceived.HasValue
                    ? new GetAsRecipientVORecipientLogin(
                        message.RecipientLoginId!.Value,
                        message.RecipientLoginName)
                    : null,
                message.Subject,
                message.Rnu,
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
