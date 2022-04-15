using EDelivery.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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
