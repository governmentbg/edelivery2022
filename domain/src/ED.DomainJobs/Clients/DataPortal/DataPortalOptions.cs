using System.ComponentModel.DataAnnotations;

namespace ED.DomainJobs
{
    public class DataPortalOptions
    {
        [Required]
        public string ApiUrl { get; set; } = null!;
        [Required]
        public string ApiKey { get; set; } = null!;
        [Required]
        public int OrganizationId { get; set; }
    }
}
