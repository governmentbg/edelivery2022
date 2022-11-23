using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IMessageOpenQueryRepository;

namespace ED.Domain
{
    partial class MessageOpenQueryRepository : IMessageOpenQueryRepository
    {
        public async Task<GetPdfAsSenderVO> GetPdfAsSenderAsync(
            int messageId,
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

                where m.MessageId == messageId

                select new
                {
                    m.MessagePdfBlobId,
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
                    RecipientProfileName = rp.ElectronicSubjectName,
                    mr.DateReceived,
                    RecipientMessageSummary = mr.MessageSummary
                })
                .ToArrayAsync(ct);

            var grouping = message.GroupBy(e => new
            {
                e.MessagePdfBlobId,
                e.SenderProfileId,
                e.SenderProfileName,
                e.DateSent,
                e.MessageSummaryVersion,
                e.Subject,
                e.Rnu,
                e.TemplateId
            },
            g => new
            {
                g.MessageSummary,
                g.Body,
                g.IV,
                g.RecipientProfileName,
                g.DateReceived,
                g.RecipientMessageSummary
            }).First();

            GetPdfAsSenderVOBlobs[] blobs = await (
                from mb in this.DbContext.Set<MessageBlob>()

                join b in this.DbContext.Set<Blob>()
                    on mb.BlobId equals b.BlobId

                where mb.MessageId == messageId

                select new GetPdfAsSenderVOBlobs(
                    b.FileName,
                    b.Hash!,
                    b.HashAlgorithm!,
                    b.Size))
                .ToArrayAsync(ct);

            GetPdfAsSenderVO vo = new(
                grouping.Key.MessagePdfBlobId,
                grouping.Key.SenderProfileId,
                grouping.Key.SenderProfileName,
                grouping.Key.DateSent!.Value,
                grouping.Key.MessageSummaryVersion,
                grouping.First().MessageSummary,
                grouping.Key.Subject,
                grouping.Key.Rnu,
                grouping.First().Body,
                grouping.Key.TemplateId,
                grouping.First().IV,
                grouping
                    .Select(g => new GetPdfAsSenderVORecipients(
                        g.RecipientProfileName,
                        g.DateReceived,
                        g.RecipientMessageSummary))
                    .ToArray(),
                blobs);

            return vo;
        }
    }
}
