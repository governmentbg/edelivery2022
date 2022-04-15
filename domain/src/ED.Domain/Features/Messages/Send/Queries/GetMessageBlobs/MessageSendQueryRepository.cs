using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IMessageSendQueryRepository;

namespace ED.Domain
{
    partial class MessageSendQueryRepository : IMessageSendQueryRepository
    {
        public async Task<GetMessageBlobsVO[]> GetMessageBlobsAsync(
            int profileId,
            int messageId,
            int[] blobIds,
            CancellationToken ct)
        {
            GetMessageBlobsVO[] vos = await (
                from mb in this.DbContext.Set<MessageBlob>()

                join mbak in this.DbContext.Set<MessageBlobAccessKey>()
                    on mb.MessageBlobId equals mbak.MessageBlobId

                join pk in this.DbContext.Set<ProfileKey>()
                    on mbak.ProfileKeyId equals pk.ProfileKeyId

                where mb.MessageId == messageId
                    && mbak.ProfileId == profileId
                    && this.DbContext.MakeIdsQuery(blobIds).Any(id => id.Id == mb.BlobId)

                select new GetMessageBlobsVO(
                    mb.MessageBlobId,
                    pk.ProfileKeyId,
                    pk.Provider,
                    pk.KeyName,
                    pk.OaepPadding,
                    mbak.EncryptedKey)
                )
                .ToArrayAsync(ct);

            return vos;
        }
    }
}
