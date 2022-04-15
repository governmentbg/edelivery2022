using System;

namespace ED.Domain
{
    public partial interface IProfileServiceQueryRepository
    {
        public record GetActiveProfileKeyVO(
            int ProfileKeyId,
            DateTime ExpiresAt,
            string Provider,
            string KeyName,
            string OaepPadding);
    }
}
