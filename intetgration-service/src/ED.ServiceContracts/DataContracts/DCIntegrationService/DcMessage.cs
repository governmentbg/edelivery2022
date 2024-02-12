using System;
using System.Runtime.Serialization;

namespace EDelivery.Common.DataContracts
{
    [DataContract]
    public class DcMessage
    {
        public DcMessage()
        {

        }
        public DcMessage(WebDcMessage m)
        {
            if (m == null)
                return;
            this.Id = m.Id;
            this.IsDraft = m.IsDraft;
            this.DateCreated = m.DateCreated;
            this.DateSent = m.DateSent;
            this.DateReceived = m.DateReceived;
            this.ReceiverLogin = m.ReceiverLogin;
            this.ReceiverProfile = m.ReceiverProfile;
            if (m.ForwardedMessage != null && m.ForwardStatus == Enums.eForwardStatus.IsInForwardChain)
            {
                this.SenderProfile = m.ForwardedMessage.SenderProfile;
                this.SenderLogin = m.ForwardedMessage.SenderLogin;
                this.Title = m.ForwardedMessage.Title;
            }
            else
            {
                this.Title = m.Title;
                this.SenderLogin = m.SenderLogin;
                this.SenderProfile = m.SenderProfile;
            }

        }

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public bool IsDraft { get; set; }

        [DataMember]
        public DateTime DateCreated { get; set; }

        [DataMember]
        public DateTime? DateSent { get; set; }

        [DataMember]
        public DateTime? DateReceived { get; set; }

        [DataMember]
        public DcLogin SenderLogin { get; set; }

        [DataMember]
        public DcLogin ReceiverLogin { get; set; }

        [DataMember]
        public DcProfile SenderProfile { get; set; }

        [DataMember]
        public DcProfile ReceiverProfile { get; set; }
    }
}
