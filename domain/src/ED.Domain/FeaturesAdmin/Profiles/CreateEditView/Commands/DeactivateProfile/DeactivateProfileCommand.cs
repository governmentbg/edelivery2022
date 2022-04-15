using MediatR;

namespace ED.Domain
{
    public record DeactivateProfileCommand(
        int ProfileId,
        int AdminUserId,
        string Ip)
        : IRequest<DeactivateProfileCommandResult>;

    public record DeactivateProfileCommandResult(
        bool IsSuccessful,
        string Error);
}
