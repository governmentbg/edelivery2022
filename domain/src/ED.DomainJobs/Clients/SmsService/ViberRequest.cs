#pragma warning disable IDE1006 // Naming Styles

namespace ED.DomainJobs
{
    public class ViberRequest
    {
        public int cid { get; set; }

        public int sid { get; set; }

        public int priority { get; set; }

        public string? clientMsgId { get; set; }

        public ViberRequestRecipient recipient { get; set; } = null!;

        public ViberRequestRecipientMessage message { get; set; } = null!;
    }

    public class ViberRequestRecipient
    {
        public string? msisdn { get; set; }
    }

    public class ViberRequestRecipientMessage
    {
        public ViberRequestRecipientMessageText? sms { get; set; }

        public ViberRequestRecipientMessageText? viber { get; set; }
    }

    public class ViberRequestRecipientMessageText
    {
        public string? text { get; set; }
    }
}
