using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcPdfStampMessageContent
    {
        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public string ContentType { get; set; }

        [DataMember]
        public byte[] Content { get; set; }
    }
}
