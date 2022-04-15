using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDelivery.SEOS.DBEntities
{
    [Table("AS4SentMessagesStatus")]
    public class AS4SentMessagesStatus
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string AS4Guid { get; set; }

        [Required]
        [StringLength(50)]
        public string MessageGuid { get; set; }

        [Required]
        [StringLength(50)]
        public string MessageType { get; set; }

        [StringLength(50)]
        public string DocGuid { get; set; }

        [Required]
        [StringLength(50)]
        public string SenderIdentifier { get; set; }

        [Required]
        [StringLength(50)]
        public string ReceiverIdentifier { get; set; }

        [Required]
        public int Status { get; set; }

        public DateTime? DateSent { get; set; }

        public DateTime? DateStatusChange { get; set; }

        [Required]
        public bool SentResponse { get; set; }
    }
}
