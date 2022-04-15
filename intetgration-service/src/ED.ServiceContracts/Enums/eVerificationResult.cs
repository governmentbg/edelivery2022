using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace EDelivery.Common.Enums
{
    [DataContract]
    public enum eVerificationResult:int
    {
        [EnumMember]
        Success = 0,
        [EnumMember]
        InvalidFile = 1,
        [EnumMember]
        NoSignatureFound = 2,
        [EnumMember]
        DetachedSignature=3,

    }
}