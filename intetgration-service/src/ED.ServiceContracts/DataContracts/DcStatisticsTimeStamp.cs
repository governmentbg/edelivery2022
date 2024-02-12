using System;
using System.Runtime.Serialization;

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
