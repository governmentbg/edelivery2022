using System;

namespace ED.Domain
{
    public partial interface IEsbMessagesOpenQueryRepository
    {
        public record GetOpenMessageInfoVO(
            DateTime DateReceived,
            int LoginId,
            string LoginName);
    }
}
