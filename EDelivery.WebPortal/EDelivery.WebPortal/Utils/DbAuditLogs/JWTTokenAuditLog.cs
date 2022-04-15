using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EDelivery.WebPortal.Utils.DbAuditLogs
{
    [Table("JWTTokenAuditLog")]
    public class JWTTokenAuditLog
    {
        [Key]
        public int Id { get; set; }
        public int? ProfileId { get; set; }
        public Guid? ProfileElectronicSubjectId { get; set; }
        public int? LoginId { get; set; }
        public string Username { get; set; }
        public int TokenType { get; set; }
        public string Token { get; set; }
        public Guid TokenJti { get; set; }
        public DateTime DateCreated { get; set; }
    }
}