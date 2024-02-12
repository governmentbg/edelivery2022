#pragma warning disable IDE1006 // Naming Styles

namespace ED.DomainJobs
{
    public class SmsRequest
    {
        public int cid { get; set; }

        public int sid { get; set; }

        public int priority { get; set; }

        public SmsRequestRecipient? recipient { get; set; }

        public SmsRequestMessage? message { get; set; }
    }

    public class SmsRequestRecipient
    {
        public string? msisdn { get; set; }
    }

    public class SmsRequestMessage
    {
        public SmsRequestMessageText? sms { get; set; }
    }

    public class SmsRequestMessageText
    {
        public string? text { get; set; }
    }
}
