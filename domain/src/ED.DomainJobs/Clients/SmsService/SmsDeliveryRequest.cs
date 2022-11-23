namespace ED.DomainJobs
{
    public class SmsDeliveryRequest
    {
#pragma warning disable CA1707 // Identifiers should not contain underscores
        public string? sms_id { get; set; }

        public string? service_id { get; set; }
#pragma warning restore CA1707 // Identifiers should not contain underscores
    }
}
