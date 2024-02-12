using MediatR;

namespace ED.Domain
{
    public record UpdateIndividualNamesCommand(
        int ProfileId,
        string FirstName,
        string MiddleName,
        string LastName,
        int ActionLoginId,
        string Ip)
        : IRequest;
}
