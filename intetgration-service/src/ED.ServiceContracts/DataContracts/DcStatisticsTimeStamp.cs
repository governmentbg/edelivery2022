using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcStatisticsTimeStamp
    {
        [DataMember]
        public DateTime FromDate { get; set; }
        [DataMember]
        public DateTime ToDate { get; set; }
        [DataMember]
        public int SuccessfulTimeStampRequestsCount { get; set; }
        [DataMember]
        public int FailedTimeStampRequestsCount { get; set; }
    }
}
