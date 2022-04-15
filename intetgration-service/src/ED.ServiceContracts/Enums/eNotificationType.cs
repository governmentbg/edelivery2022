using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Common.Enums
{
    [DataContract]
    public enum eNotificationType:byte
    {
        [EnumMember]
        Email=0,
        [EnumMember]
        SMS=1,
        [EnumMember]
        PushNotification = 2
    }
}
