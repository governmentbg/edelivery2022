using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Common.Enums
{
    /// <summary>
    /// Enumerates all statuses, available for administration requests
    /// </summary>
    [DataContract]
    public enum eRequestStatus:int
    {
        [EnumMember]
        New=1,

        [EnumMember]
        Confirmed=2,

        [EnumMember]
        Rejected=3,

        [EnumMember]
        Deleted=4
    }
}
