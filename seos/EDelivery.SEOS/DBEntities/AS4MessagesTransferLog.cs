using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;

namespace EDelivery.SEOS.DBEntities
{
    [Table("AS4MessagesTransferLog")]
    public class AS4MessagesTransferLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string MessageGUID { get; set; }

        [Required]
        [StringLength(50)]
        public string MessageType { get; set; }

        public DateTime TransferDate { get; set; }

        public short TransferType { get; set; }

        [Required]
        [StringLength(500)]
        public string SenderName { get; set; }

        [Required]
        [StringLength(50)]
        public string SenderIdentifier { get; set; }

        [Required]
        [StringLength(500)]
        public string ReceiverName { get; set; }

        [Required]
        [StringLength(50)]
        public string ReceiverIdentifier { get; set; }

        public int AttachedFilesCount { get; set; }

        public int AttachedFilesSize { get; set; }
    }
}
