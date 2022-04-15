using System.ComponentModel.DataAnnotations;

namespace ED.DomainJobs
{
    public class LinkMobilityOptions
    {
        [Required]
        public string ApiUrl { get; set; } = null!;
        [Required]
        public string ApiKey { get; set; } = null!;
        [Required]
        public string ApiSecret { get; set; } = null!;
        [Required]
        public string ServiceId { get; set; } = null!;
        [Required]
        public LinkMobilityOptionsSms Sms { get; set; } = null!;
        [Required]
        public LinkMobilityOptionsViber Viber { get; set; } = null!;
    }

    public class LinkMobilityOptionsSms
    {
        [Required]
        public string Sc { get; set; } = null!;

        [Required]
        public int MaxMessageSize { get; set; } = 160;
    }

    public class LinkMobilityOptionsViber
    {
        [Required]
        public string Sc { get; set; } = null!;
    }
}
