using System;

namespace ED.Domain
{
    public partial interface IMessageOpenQueryRepository
    {
        public record GetMessageRecipientsVO(
           string ProfileName,
           DateTime? DateReceived);
    }
}
