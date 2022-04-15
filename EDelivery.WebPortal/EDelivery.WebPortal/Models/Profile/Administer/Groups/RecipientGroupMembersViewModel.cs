using System.Collections.Generic;
using System.Linq;

namespace EDelivery.WebPortal.Models
{
    public class RecipientGroupMembersViewModel
    {
        public RecipientGroupMembersViewModel(
            int recipientGroupId,
            ED.DomainServices.Profiles.GetRecipientGroupMembersResponse.Types.Member[] members)
        {
            this.RecipientGroupId = recipientGroupId;

            this.Members.AddRange(members
                .Select(e => new RecipientGroupMember(e)));
        }

        public int RecipientGroupId { get; set; }

        public List<RecipientGroupMember> Members { get; set; } =
            new List<RecipientGroupMember>();
    }

    public class RecipientGroupMember
    {
        public RecipientGroupMember(
            ED.DomainServices.Profiles.GetRecipientGroupMembersResponse.Types.Member member)
        {
            this.ProfileId = member.ProfileId;
            this.ProfileName = member.ProfileName;
            this.ProfileTargetGroup = member.ProfileTargetGroup;
        }

        public int ProfileId { get; set; }

        public string ProfileName { get; set; }

        public string ProfileTargetGroup { get; set; }
    }
}