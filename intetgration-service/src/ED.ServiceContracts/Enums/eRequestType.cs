using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Common.Enums
{
    /// <summary>
    /// Type of requests, waiting for administrative action
    /// </summary>
    [DataContract]
    public enum eRequestType:int
    {
        [EnumMember]
        RegistrationWithCert = 1,

        [EnumMember]
        RegistrationWithEID = 4,

        [EnumMember]
        RegistrationWithAdministrativeAct = 5,

        [EnumMember]
        RegistrationWithToken = 6,

        [EnumMember]
        DetailsChange = 2,

        [EnumMember]
        Delete = 3
    }
}
