using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcSendFileSizeLimitsReportList
    {
        [DataMember]
        public List<DcSendFileSizeLimitsReport> TruncatedReportList { get; set; }

        [DataMember]
        public int AllRecordsCount { get; set; }

        [DataMember]
        public int PageSize { get; set; }
    }
}
