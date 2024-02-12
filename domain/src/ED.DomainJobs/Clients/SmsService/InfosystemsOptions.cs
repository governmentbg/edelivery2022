using System.ComponentModel.DataAnnotations;

namespace ED.DomainJobs
{
    public class InfosystemsOptions
    {
        [Required]
        public string ApiUrl { get; set; } = null!;
        [Required]
        public string ApiUserName { get; set; } = null!;
        [Required]
        public string ApiPassword { get; set; } = null!;
        [Required]
        public int Cid { get; set; }
    }
}
