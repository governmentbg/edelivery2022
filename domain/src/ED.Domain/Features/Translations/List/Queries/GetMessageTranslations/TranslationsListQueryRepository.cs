using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.ITranslationsListQueryRepository;

namespace ED.Domain
{
    partial class TranslationsListQueryRepository : ITranslationsListQueryRepository
    {
        public async Task<GetMessageTranslationsVO[]> GetMessageTranslationsAsync(
            int messageId,
            int profileId,
            CancellationToken ct)
        {
            GetMessageTranslationsVO[] vos = await (
                from mt in this.DbContext.Set<MessageTranslation>()

                join m in this.DbContext.Set<Message>()
                    on mt.MessageId equals m.MessageId

                where mt.ProfileId == profileId
                    && mt.MessageId == messageId
                    && mt.ArchiveDate == null

                select new GetMessageTranslationsVO(
                    mt.MessageTranslationId,
                    m.MessageId,
                    m.Subject,
                    mt.SourceLanguage,
                    mt.TargetLanguage,
                    mt.CreateDate,
                    mt.ModifyDate))
                .ToArrayAsync(ct);

            return vos;
        }
    }
}
