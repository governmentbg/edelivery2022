using System.Runtime.Serialization;

namespace EDelivery.SEOS.DataContracts
{
    [DataContract]
    public class CorespondentResponse
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public int MessageId { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string City { get; set; }
        [DataMember]
        public string Address { get; set; }
        [DataMember]
        public string EGN { get; set; }
        [DataMember]
        public string IDCard { get; set; }
        [DataMember]
        public string Bulstat { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string Phone { get; set; }
        [DataMember]
        public string MobilePhone { get; set; }
        [DataMember]
        public string MOL { get; set; }
        [DataMember]
        public string Comment { get; set; }
        [DataMember]
        public int? Kind { get; set; }
    }
}
