using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.SEOS.DBEntities
{
    [Table("SEOSMessageCorespondent")]
    public class SEOSMessageCorespondent
    {
        [Key]
        public int Id { get; set; }
        public int MessageId { get; set; }

        public string Name { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string EGN { get; set; }
        public string IDCard { get; set; }
        public string Bulstat { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string MobilePhone { get; set; }
        public string MOL { get; set; }
        public string Comment { get; set; }
        public int? Kind { get; set; }
        [ForeignKey("MessageId")]
        public virtual SEOSMessage Message { get; set; }
    }
}
