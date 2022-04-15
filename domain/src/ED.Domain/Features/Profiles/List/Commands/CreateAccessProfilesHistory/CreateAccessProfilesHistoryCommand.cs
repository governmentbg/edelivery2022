using MediatR;

namespace ED.Domain
{
    public record CreateAccessProfilesHistoryCommand(
        int ProfileId,
        int LoginId,
        string IP)
        : IRequest;
}
