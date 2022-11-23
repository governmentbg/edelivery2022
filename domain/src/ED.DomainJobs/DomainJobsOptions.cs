using System.ComponentModel.DataAnnotations;

namespace ED.DomainJobs
{
    public class DomainJobsOptions
    {
        [Required]
        public EmailJobOptions EmailJob { get; set; } = null!;
        [Required]
        public SmsJobOptions SmsJob { get; set; } = null!;
        [Required]
        public PushNotificationJobOptions PushNotificationJob { get; set; } = null!;
        [Required]
        public ViberJobOptions ViberJob { get; set; } = null!;
        [Required]
        public SmsDeliveryCheckJobOptions SmsDeliveryCheckJob { get; set; } = null!;
        [Required]
        public ViberDeliveryCheckJobOptions ViberDeliveryCheckJob { get; set; } = null!;
    }
}
