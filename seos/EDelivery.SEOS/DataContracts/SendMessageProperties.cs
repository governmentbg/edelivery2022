using System;
using System.Runtime.Serialization;

namespace EDelivery.SEOS.DataContracts
{
    [DataContract]
    public class SendMessageProperties
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public Guid MessageGuid { get; set; }
        [DataMember]
        public DocumentIdentificationType DocIdentity { get; set; }
        [DataMember]
        public string SenderCertificateSN { get; set; }
        [DataMember]
        public EntityNodeType Sender { get; set; }
        [DataMember]
        public int ReceiverId { get; set; }
        [DataMember]
        public string ReceiverServiceUrl { get; set; }
        [DataMember]
        public EntityNodeType Receiver { get; set; }
        [DataMember]
        public string MessageXml { get; set; }
    }
}
