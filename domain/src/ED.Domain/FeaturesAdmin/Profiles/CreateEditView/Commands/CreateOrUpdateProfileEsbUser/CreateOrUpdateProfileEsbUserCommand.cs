using MediatR;

namespace ED.Domain
{
    public record CreateOrUpdateProfileEsbUserCommand(
        int ProfileId,
        string OId,
        string ClientId,
        int AdminUserId)
        : IRequest;
}
