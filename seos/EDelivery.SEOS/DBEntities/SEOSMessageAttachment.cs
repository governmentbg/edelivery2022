using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.SEOS.DBEntities
{
    [Table("SEOSMessageAttachment")]
    public class SEOSMessageAttachment
    {
        [Key]
        public int Id { get; set; }
        public int? MessageId { get; set; }
        public string Name { get; set; }
        public byte[] Content { get; set; }
        public string MimeType { get; set; }
        public string Comment { get; set; }
        public int? MalwareScanResultId { get; set; }
        [ForeignKey("MessageId")]
        public virtual SEOSMessage Message { get; set; }
        [ForeignKey("MalwareScanResultId")]
        public virtual MalwareScanResult MalwareScanResult { get; set; }
    }
}
