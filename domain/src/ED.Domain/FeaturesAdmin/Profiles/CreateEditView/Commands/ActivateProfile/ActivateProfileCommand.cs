using MediatR;

namespace ED.Domain
{
    public record ActivateProfileCommand(
        int ProfileId,
        int AdminUserId,
        string Ip)
        : IRequest<ActiveProfileCommandResult>;

    public record ActiveProfileCommandResult(
        bool IsSuccessful,
        string Error);
}
