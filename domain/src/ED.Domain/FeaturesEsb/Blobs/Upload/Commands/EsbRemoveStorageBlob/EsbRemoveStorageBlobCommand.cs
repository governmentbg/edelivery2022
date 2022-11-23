using MediatR;

namespace ED.Domain
{
    public record EsbRemoveStorageBlobCommand(
        int ProfileId,
        int BlobId
    ) : IRequest;
}
