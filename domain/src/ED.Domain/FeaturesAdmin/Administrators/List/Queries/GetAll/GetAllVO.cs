using System;

namespace ED.Domain
{
    public partial interface IAdminAdministratorsListQueryRepository
    {
        public record GetAllVO(
            int Id,
            string Name,
            DateTime CreatedOn,
            string CreatedBy,
            string DisabledBy,
            bool IsActive);
    }
}
