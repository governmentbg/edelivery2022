using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.SEOS.DBEntities
{
    [Table("SEOSRetrySendQueue")]
    public class SEOSRetrySendQueueItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MessageId { get; set; }
        public int ReceiverId { get; set; }
        public string MessageBody { get; set; }
        public int RetryCount { get; set; }
        public DateTime DateFirstSent { get; set; }
        public DateTime? DateLastRetry { get; set; }

        [ForeignKey("ReceiverId")]
        public virtual RegisteredEntity Receiver { get; set; }

    }
}
