using System;

namespace ED.Domain
{
    public partial interface IAdminAdministratorsCreateEditViewQueryRepository
    {
        public record GetAdministratorVO(
            int Id,
            string FirstName,
            string MiddleName,
            string LastName,
            string Identifier,
            string Phone,
            string Email,
            string UserName,
            bool IsActive,
            DateTime CreatedOn,
            string CreatedBy,
            DateTime? DisabledOn,
            string DisabledBy,
            string DisableReason);
    }
}
