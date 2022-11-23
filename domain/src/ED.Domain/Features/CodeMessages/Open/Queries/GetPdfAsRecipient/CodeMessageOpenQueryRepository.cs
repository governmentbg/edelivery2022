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
        public async Task<GetPdfAsRecipientVO> GetPdfAsRecipientAsync(
            Guid accessCode,
            CancellationToken ct)
        {
            var message = await (
                from mac in this.DbContext.Set<MessagesAccessCode>()

                join m in this.DbContext.Set<Message>()
                    on mac.MessageId equals m.MessageId

                join sp in this.DbContext.Set<Profile>()
                    on m.SenderProfileId equals sp.Id

                join mr in this.DbContext.Set<MessageRecipient>()
                   on m.MessageId equals mr.MessageId

                join rp in this.DbContext.Set<Profile>()
                    on mr.ProfileId equals rp.Id

                where mac.AccessCode == accessCode

                select new
                {
                    m.MessageId,
                    mr.MessagePdfBlobId,
                    SenderProfileId = m.SenderProfileId,
                    SenderProfileName = sp.ElectronicSubjectName,
                    DateSent = m.DateSent,
                    m.MessageSummaryVersion,
                    m.MessageSummary,
                    m.Subject,
                    m.Rnu,
                    m.Body,
                    m.TemplateId,
                    m.IV,
                    RecipientProfileId = mr.ProfileId,
                    RecipientProfileName = rp.ElectronicSubjectName,
                    mr.DateReceived,
                    RecipientMessageSummary = mr.MessageSummary
                })
                .SingleAsync(ct);

            GetPdfAsRecipientVOBlobs[] blobs = await (
                from mb in this.DbContext.Set<MessageBlob>()

                join b in this.DbContext.Set<Blob>()
                    on mb.BlobId equals b.BlobId

                where mb.MessageId == message.MessageId

                select new GetPdfAsRecipientVOBlobs(
                    b.FileName,
                    b.Hash!,
                    b.HashAlgorithm!,
                    b.Size))
                .ToArrayAsync(ct);

            GetPdfAsRecipientVO vo = new(
                message.MessageId,
                message.MessagePdfBlobId,
                message.SenderProfileId,
                message.SenderProfileName,
                message.DateSent!.Value,
                message.MessageSummaryVersion,
                message.MessageSummary,
                message.Subject,
                message.Rnu,
                message.Body,
                message.TemplateId,
                message.IV,
                new GetPdfAsRecipientVORecipient(
                    message.RecipientProfileId,
                    message.RecipientProfileName,
                    message.DateReceived!.Value,
                    message.RecipientMessageSummary!),
                blobs);

            return vo;
        }
    }
}
