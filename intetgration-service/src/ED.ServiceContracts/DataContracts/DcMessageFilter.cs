using System;

namespace EDelivery.Common.DataContracts
{
    public class DcMessageFilter
    {
        public string SearchInTitle { get; set; }
        public string SearchInElectronicSubject { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
