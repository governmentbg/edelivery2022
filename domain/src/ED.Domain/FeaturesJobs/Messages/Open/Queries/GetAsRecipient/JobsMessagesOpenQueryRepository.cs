using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IJobsMessagesOpenQueryRepository;

namespace ED.Domain
{
    partial class JobsMessagesOpenQueryRepository : IJobsMessagesOpenQueryRepository
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

                join mak in this.DbContext.Set<MessageAccessKey>()
                    on m.MessageId equals mak.MessageId

                join t in this.DbContext.Set<Template>()
                    on m.TemplateId equals t.TemplateId

                where m.MessageId == messageId
                    && mr.ProfileId == profileId
                    && mak.ProfileId == profileId

                select new
                {
                    m.MessageId,
                    DateSent = m.DateSent!.Value,
                    mr.DateReceived,
                    SenderProfileName = sp.ElectronicSubjectName,
                    t.TemplateId,
                    m.Subject,
                    m.Rnu,
                    m.Body,
                    m.ForwardStatusId,
                    mak.ProfileKeyId,
                    mak.EncryptedKey,
                    m.IV,
                    TemplateName = t.Name,
                })
                .FirstAsync(ct);

            var blobs = await (
                from mb in this.DbContext.Set<MessageBlob>()

                join b in this.DbContext.Set<Blob>()
                    on mb.BlobId equals b.BlobId

                where mb.MessageId == messageId

                select new
                {
                    b.BlobId,
                    b.FileName,
                    b.Size,
                    b.DocumentRegistrationNumber,
                    b.Hash,
                    b.HashAlgorithm,
                })
                .ToArrayAsync(ct);

            GetAsRecipientVO vo = new(
                message.MessageId,
                message.DateSent,
                message.DateReceived,
                message.SenderProfileName,
                message.TemplateId,
                message.Subject,
                message.Rnu,
                message.Body,
                message.ProfileKeyId,
                message.EncryptedKey,
                message.IV,
                message.TemplateName,
                blobs
                    .Select(e => new GetAsRecipientVOBlob(
                        e.BlobId,
                        e.FileName,
                        e.Hash,
                        e.HashAlgorithm,
                        e.Size,
                        e.DocumentRegistrationNumber))
                    .ToArray());

            return vo;
        }
    }
}
