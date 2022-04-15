using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Common.Enums
{
    [DataContract]
    public enum eForwardStatus:int
    {
        [EnumMember]
        None=0,
        [EnumMember]
        IsOriginalForwarded =1,
        [EnumMember]
        IsInForwardChain =2,
        [EnumMember]
        IsInForwardChainAndForwarded =3

    }
}
