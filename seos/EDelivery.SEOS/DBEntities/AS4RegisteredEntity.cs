using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.SEOS.DBEntities
{
    [Table("AS4RegisteredEntity")]
    public class AS4RegisteredEntity
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public string EIK { get; set; }

        [Required]
        public string AS4Node { get; set; }
    }
}
