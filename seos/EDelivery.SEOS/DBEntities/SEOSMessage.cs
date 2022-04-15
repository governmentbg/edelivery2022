using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.SEOS.DBEntities
{
    [Table("SEOSMessage")]
    public class SEOSMessage
    {
        public SEOSMessage()
        {
            this.Attachments = new HashSet<SEOSMessageAttachment>();
            this.Corespondents = new HashSet<SEOSMessageCorespondent>();
        }
        [Key]
        public int Id { get; set; }
        public int? ParentMessageId { get; set; }
        /// <summary>
        /// Number from the original doc system (only in case of sending document)
        /// </summary>
        public string DocReferenceNumber { get; set; }

        public int SenderEntityId { get; set; }
        public Guid? SenderElectronicSubjectId { get; set; }
        public string SenderLoginName { get; set; }
        public Guid? SenderLoginElectronicSubjectId { get; set; }

        public int ReceiverEntityId { get; set; }
        public Guid? ReceiverElectronicSubjectId { get; set; }
        public string ReceiverLoginName { get; set; }
        public Guid? ReceiverLoginElectronicSubjectId { get; set; }

        public Guid MessageGuid { get; set; }
        public DateTime MessageDate { get; set; }
        public string MessageComment { get; set; }
        public string DocAbout { get; set; }
        public string DocKind { get; set; }
        public Guid DocGuid { get; set; }
        public string DocNumberExternal { get; set; }
        public DateTime? DocDateExternal { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string  DocNumberInternal { get; set; }
        public Guid? ParentDocGuid { get; set; }
        public string ParentDocNumber { get; set; }
        public string DocComment { get; set; }
        public string DocAddData { get; set; }
        public DateTime? DocReqDateClose { get; set; }
        public string DocAttentionTo { get; set; }
        public int Status { get; set; }
        public string RejectedReason { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public DateTime? DateRegistered { get; set; }
        public string ServiceName { get; set; }
        public string ServiceType { get; set; }
        public string ServiceCode { get; set; }
        public DateTime? DocExpectCloseDate { get;  set; }
        public bool IsInitiatingDoc { get; set; }

        [ForeignKey("SenderEntityId")]
        public virtual RegisteredEntity Sender { get; set; }
        [ForeignKey("ReceiverEntityId")]
        public virtual RegisteredEntity Receiver { get; set; }

        public virtual ICollection<SEOSMessageCorespondent> Corespondents { get; set; }
        public virtual ICollection<SEOSMessageAttachment> Attachments { get; set; }
    }
}
