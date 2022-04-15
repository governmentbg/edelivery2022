using System.ComponentModel.DataAnnotations;

namespace ED.DomainJobs
{
    public class ViberJobOptions : QueueJobOptions
    {
        [Required]
        public string ViberUrl { get; set; } = null!;
        [Required]
        public string ViberId { get; set; } = null!;
        [Required]
        public string ViberSc { get; set; } = null!;
        [Required]
        public string ViberKey { get; set; } = null!;
        [Required]
        public string ViberSecret { get; set; } = null!;
    }
}
