using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class WebDcForwardedMessageHistory
    {

        [DataMember]
        public int MessageId { get; set; }
        [DataMember]
        public string ReceiverElectronicSubjectName { get; set; }
        [DataMember]
        public DateTime DateSent { get; set; }
        [DataMember]
        public DateTime? DateReceived { get; set; }
        [DataMember]
        public string SenderElectronicSubjectName { get; set; }
    }
}
