using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.SEOS.DBEntities
{
    [Table("SEOSCommunicationLog")]
    public class SEOSCommunicationLog
    {
        [Key]
        public int Id { get; set; }
        public Guid? MessageGuid { get; set; }
        public DateTime DateCreated { get; set; }
        public string MessageBody { get; set; }
        public string MessageType { get; set; }
        public bool IsRequest { get; set; }
        public bool IsInbound { get; set; }
        public string ReceiverUrl { get; set; }
    }
}
