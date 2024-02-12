using MediatR;

namespace ED.Domain
{
    public record RevokeProfileAccessByAdminCommand(
        int ProfileId,
        int LoginId,
        int AdminUserId,
        string Ip)
        : IRequest;
}
