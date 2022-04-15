using System.Runtime.Serialization;

namespace EDelivery.SEOS.DataContracts
{
    [DataContract]
    public class AttachmentRequest
    {
        [DataMember]
        public int? Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public byte[] Content { get; set; }
        [DataMember]
        public string MimeType { get; set; }
        [DataMember]
        public string Comment { get; set; }
    }
}
