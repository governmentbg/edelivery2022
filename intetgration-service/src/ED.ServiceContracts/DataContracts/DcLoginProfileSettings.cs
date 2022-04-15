using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcLoginProfileSettings
    {
        [DataMember]
        public bool EnableEmailNotifications { get; set; }

        [DataMember]
        public bool EnableSmsNotifications { get; set; }

        [DataMember]
        public bool  EnableEmailNotificationsOnDelivery {get;set;}
    }
}
