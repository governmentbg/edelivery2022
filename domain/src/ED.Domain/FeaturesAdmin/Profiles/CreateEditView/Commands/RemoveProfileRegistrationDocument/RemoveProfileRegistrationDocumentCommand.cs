using MediatR;

namespace ED.Domain
{
    public record RemoveProfileRegistrationDocumentCommand(
        int ProfileId,
        int BlobId)
        : IRequest;
}
