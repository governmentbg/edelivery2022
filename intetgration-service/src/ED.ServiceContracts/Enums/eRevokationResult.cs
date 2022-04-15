using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace EDelivery.Common.Enums
{
    [DataContract]
    public enum eRevokationResult:int
    {
        [EnumMember]
        OK = 0,
        [EnumMember]
        Revoked = 1,
        [EnumMember]
        CanNotDetermine = 2
    }
}