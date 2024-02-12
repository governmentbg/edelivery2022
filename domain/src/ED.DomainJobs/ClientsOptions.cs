using System.ComponentModel.DataAnnotations;

namespace ED.DomainJobs
{
    public class ClientsOptions
    {
        [Required]
        public InfosystemsOptions Infosystems { get; set; } = null!;

        [Required]
        public DataPortalOptions DataPortal { get; set; } = null!;
    }
}
