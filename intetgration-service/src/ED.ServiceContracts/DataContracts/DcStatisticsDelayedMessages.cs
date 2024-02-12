using System;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcStatisticsDelayedMessages
    {
        [DataMember]
        public Guid ReceiverElectronicSubjectId { get; set; }
        [DataMember]
        public string ReceiverElectronicSubjectName { get; set; }
        [DataMember]
        public Guid SenderElectronicSubjectId { get; set; }
        [DataMember]
        public string SenderElectronicSubjectName { get; set; }
        [DataMember]
        public string MessageSubject { get; set; }
        [DataMember]
        public DateTime MessageDateSent { get; set; }
        [DataMember]
        public int DaysDelayed { get; set; }
    }
}
