using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.SEOS.DBEntities
{
    [Table("SEOSStatusHistory")]
    public class SEOSStatusHistory
    {
        [Key]
        public int Id { get; set; }
        public int MessageId { get; set; }
        public int Status { get; set; }
        public DateTime DateCreated { get; set; }
        public int? UpdatedByLoginId { get; set; }
        public DateTime? ExpectedDateClose { get; set; }
        public string RejectReason { get; set; }

    }
}
