using MediatR;

namespace ED.Domain
{
    public record CreateLegalEntityCommand(
        string Name,
        string Identifier,
        string Phone,
        string Email,
        string Residence,
        int TargetGroupId,
        int? BlobId,
        int AdminUserId,
        string Ip)
        : IRequest<CreateLegalEntityCommandResult>;

    public record CreateLegalEntityCommandResult(
        int? ProfileId,
        bool IsSuccessful,
        string Error);
}
