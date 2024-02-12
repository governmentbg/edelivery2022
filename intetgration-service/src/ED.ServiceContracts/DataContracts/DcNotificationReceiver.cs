namespace EDelivery.Common.DataContracts
{
    public class DcNotificationReceiver
    {
        public bool NotifyEmail { get; set; }
        public bool NotifyPhone { get; set; }
        public string NotifPushUrl { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Name { get; set; }
    }
}
