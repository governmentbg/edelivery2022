using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcStatisticsMessagesPerMonth
    {
        [DataMember]
        public DateTime Date { get; set; }
        [DataMember]
        public string DateString { get; set; }
        [DataMember]
        public int MessagesSent { get; set; }
        [DataMember]
        public int MessagesReceived { get; set; }
    }
}
