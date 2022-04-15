using MediatR;

namespace ED.Domain
{
    public record AddProfileRegistrationDocumentCommand(
        int ProfileId,
        int AdminUserId,
        int BlobId)
        : IRequest;
}
