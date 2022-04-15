using MediatR;

namespace ED.Domain
{
    public record UpdateProfileDataCommand(
        int ProfileId,
        int AdminUserId,
        UpdateProfileDataCommandIndividualData? IndividualData,
        UpdateProfileDataCommandLegalEntityData? LegalEntityData,
        string Identifier,
        string Phone,
        string EmailAddress,
        string? AddressCountryCode,
        string? AddressState,
        string? AddressCity,
        string? AddressResidence,
        int TargetGroupId,
        bool? EnableMessagesWithCode,
        string Ip)
        : IRequest<UpdateProfileDataCommandResult>;

    public record UpdateProfileDataCommandResult(
        bool IsSuccessful,
        string Error);

    public record UpdateProfileDataCommandIndividualData(
        string FirstName,
        string MiddleName,
        string LastName);

    public record UpdateProfileDataCommandLegalEntityData(
        string Name);
}
