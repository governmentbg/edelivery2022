using System;

namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        public record GetLoginsVO(
            int LoginId,
            string LoginName,
            string AccessGrantedByLoginName,
            DateTime DateAccessGranted,
            bool IsDefault);
    }
}
