using System;
using System.Runtime.Serialization;

namespace EDelivery.SEOS.DataContracts
{
    [DataContract]
    public class AttachmentResponse
    {
        [DataMember]
        public int? Id { get; set; }
        [DataMember]
        public int? MessageId { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public byte[] Content { get; set; }
        [DataMember]
        public string MimeType { get; set; }
        [DataMember]
        public string Comment { get; set; }
        [DataMember]
        public Guid? ReceiverElectronicSubjectId { get; set; }
        [DataMember]
        public Guid? SenderElectronicSubjectId { get; set; }
        [DataMember]
        public int? MalwareScanResultId { get; set; }
        [DataMember]
        public MalwareScanResultResponse MalwareScanResult { get; set; }
    }
}
