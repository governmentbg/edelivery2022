using MediatR;

namespace ED.Domain
{
    public record EsbCreateOrUpdateIndividualCommand(
        string Identifier,
        string FirstName,
        string MiddleName,
        string LastName,
        string Phone,
        string Email,
        EsbCreateOrUpdateIndividualCommandAddress Address,
        bool IsEmailNotificationEnabled,
        bool IsFullFeatured,
        int ActionLoginId,
        string Ip)
        : IRequest<int>;

    public record EsbCreateOrUpdateIndividualCommandAddress(
        string Residence,
        string? City,
        string? State,
        string? CountryIso);
}
