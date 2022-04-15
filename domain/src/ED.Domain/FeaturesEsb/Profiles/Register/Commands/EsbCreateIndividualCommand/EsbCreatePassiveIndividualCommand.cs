using MediatR;

namespace ED.Domain
{
    public record EsbCreatePassiveIndividualCommand(
        string FirstName,
        string MiddleName,
        string LastName,
        string Identifier,
        string Phone,
        string Email,
        string Residence,
        int ActionLoginId,
        string IP)
        : IRequest<int>;
}
