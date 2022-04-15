using System;
using System.Runtime.Serialization;

namespace EDelivery.SEOS.DataContracts
{
    public class RedirectStatusRequestResult : SubmitStatusRequestResult
    {
        public Guid MessageGuid { get; set; }
        public MessageType MsgType { get; set; }
        public DocumentIdentificationType DocIdentity { get; set; }
        public EntityNodeType Sender { get; set; }
        public EntityNodeType Receiver { get; set; }
    }
}
