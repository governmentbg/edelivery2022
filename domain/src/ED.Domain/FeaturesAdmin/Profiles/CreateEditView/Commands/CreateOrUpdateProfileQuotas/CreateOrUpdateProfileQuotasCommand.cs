using MediatR;

namespace ED.Domain
{
    public record CreateOrUpdateProfileQuotasCommand(
        int ProfileId,
        int AdminUserId,
        int? StorageQuotaInMb,
        string Ip)
        : IRequest;
}
