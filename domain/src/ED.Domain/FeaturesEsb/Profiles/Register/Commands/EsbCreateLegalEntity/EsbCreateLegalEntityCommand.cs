using MediatR;

namespace ED.Domain
{
    public record EsbCreateLegalEntityCommand(
        string Identifier,
        string Name,
        string Phone,
        string Email,
        EsbCreateLegalEntityCommandAddress Address,
        int TargetGroupId,
        EsbCreateLegalEntityCommandOwnerData[] OwnersData,
        int ActionLoginId,
        string Ip)
        : IRequest<int>;

    public record EsbCreateLegalEntityCommandOwnerData(
        string Identifier,
        string Email,
        string Phone);

    public record EsbCreateLegalEntityCommandAddress(
        string Residence,
        string? City,
        string? State,
        string? CountryIso);
}
