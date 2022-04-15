using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace ED.Blobs
{
    public class ProfileBlobStreamResult : BlobStreamResult
    {
        public ProfileBlobStreamResult(int profileId, int blobId)
            : base(blobId)
        {
            this.ProfileId = profileId;
        }

        public int ProfileId { get; init; }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            context = context ?? throw new ArgumentNullException(nameof(context));

            var executor = context.HttpContext.RequestServices
                .GetRequiredService<IActionResultExecutor<ProfileBlobStreamResult>>();
            return executor.ExecuteAsync(context, this);
        }
    }
}
