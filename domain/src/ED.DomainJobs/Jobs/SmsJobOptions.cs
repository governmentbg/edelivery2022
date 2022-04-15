using System.ComponentModel.DataAnnotations;

namespace ED.DomainJobs
{
    public class SmsJobOptions : QueueJobOptions
    {
        [Required]
        public string SmsUrl { get; set; } = null!;
        [Required]
        public string SmsId { get; set; } = null!;
        [Required]
        public string SmsSc { get; set; } = null!;
        [Required]
        public string SmsKey { get; set; } = null!;
        [Required]
        public string SmsSecret { get; set; } = null!;
        [Required]
        public int SmsMaxMessageSize { get; set; } = 160;
    }
}
