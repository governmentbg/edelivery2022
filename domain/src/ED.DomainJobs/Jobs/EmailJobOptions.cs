using System.ComponentModel.DataAnnotations;

namespace ED.DomainJobs
{
    public class EmailJobOptions : QueueJobOptions
    {
        [Required]
        public string MailServer { get; set; } = null!;

        [Required]
        public string MailSender { get; set; } = null!;

        public string? MailServerUsername { get; set; }

        public string? MailServerPassword { get; set; }

        public string? MailServerDomain { get; set; }
    }
}
