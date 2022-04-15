using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Common.Enums
{
    public enum eNotificationPurpose
    {
        Received,
        Activated,
        Rejected,
        Delivered,
        ReceivedAndRegister,
        SentAndRegister,
        ReceivedWithCode,
    }
}
