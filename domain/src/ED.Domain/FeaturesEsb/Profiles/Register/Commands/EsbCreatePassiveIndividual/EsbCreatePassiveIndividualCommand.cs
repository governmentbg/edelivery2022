using MediatR;

namespace ED.Domain
{
    public record EsbCreatePassiveIndividualCommand(
        string Identifier,
        string FirstName,
        string MiddleName,
        string LastName,
        string Phone,
        string Email,
        EsbCreatePassiveIndividualCommandAddress Address,
        int ActionLoginId,
        string Ip)
        : IRequest<int>;

    public record EsbCreatePassiveIndividualCommandAddress(
        string Residence,
        string? City,
        string? State,
        string? CountryIso);
}
