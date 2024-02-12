using System;
using System.ComponentModel.DataAnnotations;

namespace ED.DomainJobs
{
    public class QueueJobOptions
    {
        public string[] Features { get; set; } = Array.Empty<string>();

        [Required]
        public int PeriodInSeconds { get; set; }

        [Required]
        public int BatchSize { get; set; }

        [Required]
        public int ParallelTasks { get; set; }

        [Required]
        public int FailedAttemptTimeoutInMinutes { get; set; }

        [Required]
        public int MaxFailedAttempts { get; set; }
    }
}
