using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.SEOS.DBEntities
{
    [Table("RegisteredEntity")]
    public class RegisteredEntity
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public Guid UniqueId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string EIK { get; set; }

        public string Phone { get; set; }
        public string Email { get; set; }
        public string Fax { get; set; }

        /// <summary>
        /// Uri of service with id 7060efa8-f1fa-4938-8b2b-12a78c86988f (Документооборот)
        /// </summary>
        public string ServiceUrl { get; set; }

        [Required]
        public string CertificateSN { get; set; }

        [Required]
        public int Status { get; set; }
        [Required]
        public DateTime LastChange { get; set; }
        [Required]
        public DateTime DateUpdated { get; set; }

    }
}
