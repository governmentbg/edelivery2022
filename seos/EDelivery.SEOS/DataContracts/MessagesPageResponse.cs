using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDelivery.SEOS.DataContracts
{
    [DataContract]
    public class MessagesPageResponse
    {
        [DataMember]
        public List<MessageResponse> Messages { get; set; }
        [DataMember]
        public int CountAllMessages { get; set; }
    }
}
