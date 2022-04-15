using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace ED.Blobs
{
    public class MessageBlobStreamResult : BlobStreamResult
    {
        public MessageBlobStreamResult(int profileId, int messageId, int blobId)
            : base(blobId)
        {
            this.ProfileId = profileId;
            this.MessageId = messageId;
        }

        public int ProfileId { get; init; }

        public int MessageId { get; init; }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            context = context ?? throw new ArgumentNullException(nameof(context));

            var executor = context.HttpContext.RequestServices
                .GetRequiredService<IActionResultExecutor<MessageBlobStreamResult>>();
            return executor.ExecuteAsync(context, this);
        }
    }
}
