using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDelivery.SEOS.DBEntities
{
    [Table("Profiles")]
    public partial class Profiles
    {
        public int Id { get; set; }

        public bool IsActivated { get; set; }

        public int ProfileType { get; set; }

        public Guid ElectronicSubjectId { get; set; }

        [Required]
        [StringLength(255)]
        public string ElectronicSubjectName { get; set; }

        [StringLength(50)]
        public string EmailAddress { get; set; }

        [StringLength(50)]
        public string Phone { get; set; }

        public DateTime DateCreated { get; set; }

        public int CreatedBy { get; set; }

        public DateTime? DateDeleted { get; set; }

        public int? DeletedByAdminUserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Identifier { get; set; }

        public bool IsReadOnly { get; set; }

        public bool IsPassive { get; set; }

        public bool? EnableMessagesWithCode { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? ModifyDate { get; set; }

        public int? ModifiedBy { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? ActivatedDate { get; set; }

        public int? AddressId { get; set; }

        public int? ActivatedByAdminUserId { get; set; }

        public int? CreatedByAdminUserId { get; set; }

        public int? ModifiedByAdminUserId { get; set; }
    }
}
