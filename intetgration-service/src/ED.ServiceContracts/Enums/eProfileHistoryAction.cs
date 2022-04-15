using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Common.Enums
{
    [DataContract]
    public enum eProfileHistoryAction
    {
        [EnumMember]
        CreateProfile,
        [EnumMember]
        AccessProfile,
        [EnumMember]
        ProfileActivated,
        [EnumMember]
        ProfileDeactivated,
        [EnumMember]
        GrantAccessToProfile,
        [EnumMember]
        RemoveAccessToProfile,
        [EnumMember]
        ProfileUpdated
    }
}
