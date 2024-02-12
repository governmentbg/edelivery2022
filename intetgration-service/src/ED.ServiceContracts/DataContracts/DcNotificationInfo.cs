using EDelivery.Common.Enums;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts
{
    /// <summary>
    /// Represent inormation about the notiication
    /// </summary>
    [DataContract]
    public class DcNotificationInfo
    {
        [DataMember]
        public eNotificationType NotificationType { get; set; }

        [DataMember]
        public string NotificationText { get; set; }

        [DataMember]
        public bool IsHTML { get; set; }

        [DataMember]
        public string NotificationSubject { get; set; }

        public eNotificationPurpose NotificationPurpose { get; set; }

        public Dictionary<string, string> ProfileFields { get; set; }

        public int NotificationId { get; set; }
    }
}
