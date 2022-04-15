using System.ComponentModel.DataAnnotations;

namespace ED.DomainJobs
{
    public class ClientsOptions
    {
        [Required]
        public LinkMobilityOptions LinkMobility { get; set; } = null!;
    }
}
