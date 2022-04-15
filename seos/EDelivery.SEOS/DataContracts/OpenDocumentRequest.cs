using System;
using System.Runtime.Serialization;

namespace EDelivery.SEOS.DataContracts
{
    [DataContract]
    public class OpenDocumentRequest
    {
        [DataMember]
        public Guid MessageGuid { get; set; }
        [DataMember]
        public Guid ESubjectId { get; set; }
        [DataMember]
        public bool IsReceived { get; set; }
        [DataMember]
        public string ProfileName { get; set; }
        [DataMember]
        public Guid ProfileGuid { get; set; }
    }
}
