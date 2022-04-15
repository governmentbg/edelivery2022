using MediatR;

namespace ED.Domain
{
    public record RemoveProfileBlobCommand(
        int ProfileId,
        int BlobId
    ) : IRequest;
}
