namespace ED.Domain
{
    public enum QueueMessageType
    {
        Email = 1,
        Sms = 2,
        PushNotification = 3,
        Viber = 4,
        SmsDeliveryCheck = 5,
        ViberDeliveryCheck = 6,
        Translation = 7,
        TranslationClosure = 8,
        DeliveredTicket = 9,
        DataPortal = 10,
    }
}
