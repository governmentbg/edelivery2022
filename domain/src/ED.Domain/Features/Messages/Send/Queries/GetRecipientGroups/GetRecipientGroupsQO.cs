using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        [Keyless]
        public record GetRecipientGroupsQO(
            string Name,
            int RecipientGroupId);
    }
}
