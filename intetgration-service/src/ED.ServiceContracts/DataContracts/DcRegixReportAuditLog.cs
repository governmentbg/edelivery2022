using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Common.DataContracts
{
    public class DcRegixReportAuditLog
    {
        public string Data { get; set; }
        public Guid Token { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
