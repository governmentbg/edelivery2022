using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
