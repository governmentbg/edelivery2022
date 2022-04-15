using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
