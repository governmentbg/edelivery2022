#pragma warning disable IDE1006 // Naming Styles

using System;

namespace ED.DomainJobs
{
    public class SmsDeliveryResponse
    {
        public int id { get; set; }

        public DateTime receivedAt { get; set; }

        public SmsDeliveryResponseMessage[]? messages { get; set; }

        public SmsDeliveryResponseRecipient? recipient { get; set; }

        public int cid { get; set; }

        public int sid { get; set; }

        public int validity { get; set; }

        public int priority { get; set; }

        public string? error { get; set; }
    }

    public class SmsDeliveryResponseMessage
    {
        public string? channel { get; set; }

        public bool charge { get; set; }

        public int messageParts { get; set; }

        public DateTime processedAt { get; set; }

        public string? status { get; set; }

        public int statusCode { get; set; }
    }

    public class SmsDeliveryResponseRecipient
    {
        public string? msisdn { get; set; }
    }
}
