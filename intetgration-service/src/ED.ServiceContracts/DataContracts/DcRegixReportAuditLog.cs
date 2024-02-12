using System;

namespace EDelivery.Common.DataContracts
{
    public class DcRegixReportAuditLog
    {
        public string Data { get; set; }
        public Guid Token { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
