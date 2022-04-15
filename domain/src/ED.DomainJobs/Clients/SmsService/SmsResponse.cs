using System.Collections.Generic;

#pragma warning disable CA2227 // Collection properties should be read only

namespace ED.DomainJobs
{
    public class SmsResponse
    {
        public SmsResponseMeta? meta { get; set; }

        public Dictionary<string, object>? data { get; set; }
    }

    public class SmsResponseMeta
    {
        public int code { get; set; }

        public string? text { get; set; }
    }
}
