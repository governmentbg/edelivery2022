using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Common.Enums
{
    [DataContract]
    public enum eVerificationInfoType
    {
        [EnumMember]
        Certificate,

        [EnumMember]
        EID,

        [EnumMember]
        AdministrativeAct,

        [EnumMember]
        NOIToken
    }
}
