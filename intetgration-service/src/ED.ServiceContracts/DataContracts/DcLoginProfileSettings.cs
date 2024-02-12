using System.Runtime.Serialization;

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
        public bool EnableEmailNotificationsOnDelivery { get; set; }
    }
}
