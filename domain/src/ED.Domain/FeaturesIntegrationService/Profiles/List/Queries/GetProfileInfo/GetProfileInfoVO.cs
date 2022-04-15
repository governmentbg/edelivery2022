using System;

namespace ED.Domain
{
    public partial interface IIntegrationServiceProfilesListQueryRepository
    {
        public record GetProfileInfoVO(
            GetProfileInfoVOIndividual? Individual,
            GetProfileInfoVOLegalEntity? LegalEntity,
            GetProfileInfoVOAdministration? Administration);

        public record GetProfileInfoVOIndividual(
            string ProfileSubjectId,
            string ProfileName,
            string ProfileIdentifier,
            DateTime DateCreated,
            bool IsActivated,
            string Email,
            string? Phone,
            string FirstName,
            string MiddleName,
            string LastName,
            GetProfileInfoVOAddress? Address);

        public record GetProfileInfoVOLegalEntity(
            string ProfileSubjectId,
            string ProfileName,
            string ProfileIdentifier,
            DateTime DateCreated,
            bool IsActivated,
            string Email,
            string? Phone,
            string Name,
            GetProfileInfoVOIndividual? RegisteredBy,
            GetProfileInfoVOAddress? Address);

        public record GetProfileInfoVOAdministration(
            string ProfileSubjectId,
            string ProfileName,
            string ProfileIdentifier,
            DateTime DateCreated,
            bool IsActivated,
            string Email,
            string? Phone,
            string Name,
            GetProfileInfoVOAdministrationSibling? Parent,
            GetProfileInfoVOAdministrationSibling[] Children,
            GetProfileInfoVOAddress? Address);

        public record GetProfileInfoVOAdministrationSibling(
            string ProfileSubjectId,
            string ProfileName,
            bool IsActivated,
            string Email,
            string? Phone);

        public record GetProfileInfoVOAddress(
            int AddressId,
            string? Residence,
            string? City,
            string? State,
            string? Country);
    }
}
