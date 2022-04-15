using System;
using MediatR;

namespace ED.Domain
{
    public record CreateOrUpdateIndividualCommand(
        string FirstName,
        string MiddleName,
        string LastName,
        string Identifier,
        string Phone,
        string Email,
        string Residence,
        bool IsPassive,
        bool IsEmailNotificationEnabled,
        bool IsSsmsNotificationEnabled,
        bool IsViberNotificationEnabled,
        int ActionLoginId,
        string IP)
        : IRequest<CreateOrUpdateIndividualCommandResult>;

    public record CreateOrUpdateIndividualCommandResult(
        int ProfileId,
        Guid ProfileGuid,
        string ProfileName);
}
