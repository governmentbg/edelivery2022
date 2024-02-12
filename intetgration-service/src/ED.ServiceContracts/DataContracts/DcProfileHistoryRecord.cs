using EDelivery.Common.Enums;
using System;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcProfileHistoryRecord
    {
        [DataMember]
        public int RecordId { get; set; }

        [DataMember]
        public int ProfileId { get; set; }

        [DataMember]
        public eProfileHistoryAction Action { get; set; }

        [DataMember]
        public string ActionLoginName { get; set; }

        [DataMember]
        public DateTime ActionDate { get; set; }

        [DataMember]
        public string IPAddress { get; set; }

        [DataMember]
        public DcProfileHistoryRecordDetails Details { get; set; }
    }
}
