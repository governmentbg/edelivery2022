using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class WebDcForwardedMessageDetails
    {
        [DataMember]
        public WebDcMessageDetails Message { get; set; }
        [DataMember]
        public WebDcMessageDetails ForwardedMessage { get; set; }
    }
}
