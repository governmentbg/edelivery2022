using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcDocumentSize
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public long SizeInBytes { get; set; }
    }
}
