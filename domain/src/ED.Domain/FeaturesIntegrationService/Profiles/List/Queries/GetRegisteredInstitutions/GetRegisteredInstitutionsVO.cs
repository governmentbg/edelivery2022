using System;

namespace ED.Domain
{
    public partial interface IIntegrationServiceProfilesListQueryRepository
    {
        public record GetRegisteredInstitutionsVO(
            string SubjectId,
            string Identifier,
            DateTime DateCreated,
            string Name,
            string Email,
            string? Phone,
            bool IsActivated,
            GetRegisteredInstitutionsVOAddress? Address,
            GetRegisteredInstitutionsVOSibling? Parent,
            GetRegisteredInstitutionsVOSibling[] Children);

        public record GetRegisteredInstitutionsVOAddress(
            int AddressId,
            string? Residence,
            string? City,
            string? State,
            string? Country);

        public record GetRegisteredInstitutionsVOSibling(
            string SubjectId,
            string Name,
            bool IsActivated,
            string Email,
            string? Phone);
    }
}
