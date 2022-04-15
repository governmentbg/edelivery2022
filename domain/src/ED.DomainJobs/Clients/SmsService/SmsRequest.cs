#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace ED.DomainJobs
{
    public class SmsRequest
    {
        public string? msisdn { get; set; }

        public string? sc { get; set; }

        public string? text { get; set; }

        public string? service_id { get; set; }
    }
}
